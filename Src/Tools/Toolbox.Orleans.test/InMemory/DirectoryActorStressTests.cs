using System.Threading.Tasks.Dataflow;
using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Graph;
using Toolbox.Orleans.test.Application;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Orleans.test.InMemory;

public class DirectoryActorStressTests : IClassFixture<InMemoryClusterFixture>
{
    private readonly InMemoryClusterFixture _clusterFixture;
    private readonly ITestOutputHelper _output;

    public DirectoryActorStressTests(InMemoryClusterFixture clusterFixture, ITestOutputHelper output)
    {
        _clusterFixture = clusterFixture.NotNull();
        _output = output.NotNull();
    }

    private record TestContractRecord(string Name, int Age);
    private record TestLeaseRecord(string LeaseId, decimal Amount);

    [Fact]
    public async Task ScaleTest()
    {
        const int count = 1000;
        const int maxDegree = 5;
        var graphClient = _clusterFixture.Cluster.Client.GetDirectoryActor();

        await ActionParallel.Run(x => AddNodes(graphClient, x), Enumerable.Range(0, count), maxDegree);
        await ActionParallel.Run(x => AddEdges(graphClient, x), Enumerable.Range(0, count - 1), maxDegree);

        var nodeList = await graphClient.Execute("select (*);", NullScopeContext.Instance);
        nodeList.IsOk().Should().BeTrue();
        nodeList.Return().Action(x =>
        {
            x.Items.Count.Should().Be(count);
            x.Items.OfType<GraphNode>().ToArray().Action(y =>
            {
                y.Length.Should().Be(count);

                var list = y
                    .Select(z => z.Key).OrderBy(z => z)
                    .Zip(Enumerable.Range(0, count).Select(z => $"node{z}"), (o, i) => (o, i))
                    .Select(x => x.o == x.i)
                    .All(x => x == true);
            });
        });

        var edgeList = await graphClient.ExecuteBatch("select [*];", NullScopeContext.Instance);
        edgeList.IsOk().Should().BeTrue();
        edgeList.Return().Action(x =>
        {
            x.Items.Length.Should().Be(1);
            x.Items.First().Action(y =>
            {
                y.Items.Count.Should().Be(count - 1);
                y.Items.OfType<GraphEdge>().Count().Should().Be(count - 1);

                var list = y.Items.OfType<GraphEdge>()
                    .Select(z => z.FromKey).OrderBy(z => z)
                    .Zip(Enumerable.Range(0, count - 1).Select(z => $"node{z}"), (o, i) => (o, i))
                    .Select(x => x.o == x.i)
                    .All(x => x == true);
            });
        });

        (await graphClient.Execute("delete (*);", NullScopeContext.Instance)).IsOk().Should().BeTrue();

        (await graphClient.ExecuteBatch("select (*);", NullScopeContext.Instance)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Items.Length.Should().Be(1);
            x.Return().Items[0].Items.Count.Should().Be(0);
        });

        (await graphClient.ExecuteBatch("select [*];", NullScopeContext.Instance)).Action(x =>
        {
            x.IsOk().Should().BeTrue();
            x.Return().Items.Length.Should().Be(1);
            x.Return().Items[0].Items.Count.Should().Be(0);
        });
    }

    [Fact]
    public async Task ParallelTasksWithErrorRecovery()
    {
        const int count = 1000;
        const int maxDegree = 5;
        var blockConfig = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = maxDegree };
        var graphClient = _clusterFixture.Cluster.Client.GetDirectoryActor();
        int sucessCount = 0;
        int retryCount = 0;
        ManualResetEventSlim resetEvent = new ManualResetEventSlim();

        ActionBlock<int> edgeBlock = null!;
        edgeBlock = new ActionBlock<int>(async x =>
        {
            var option = await AddEdges(graphClient, x);
            if (option.IsError())
            {
                Interlocked.Increment(ref retryCount);
                await edgeBlock.SendAsync(x);
                return;
            }

            int current = Interlocked.Increment(ref sucessCount);
            if (current == count - 1)
            {
                resetEvent.Set();
            }
        }, blockConfig);

        var nodeBlock = new ActionBlock<int>(async x =>
        {
            await AddNodes(graphClient, x);
            await edgeBlock.SendAsync(x);
        }, blockConfig);

        Enumerable.Range(0, count).Shuffle().ForEach(x => nodeBlock.Post(x).Should().BeTrue());

        resetEvent.Wait();
        nodeBlock.Complete();
        await nodeBlock.Completion;

        edgeBlock.Complete();
        await edgeBlock.Completion;

        (await graphClient.Execute("select (*);", NullScopeContext.Instance))
            .ThrowOnError().Return()
            .Items.Count.Should().Be(count);

        (await graphClient.Execute("select [*];", NullScopeContext.Instance))
            .ThrowOnError().Return()
            .Items.Count.Should().Be(count - 1);

        (await graphClient.Execute("delete (*);", NullScopeContext.Instance)).IsOk().Should().BeTrue();

        (await graphClient.Execute("select (*);", NullScopeContext.Instance))
            .ThrowOnError().Return()
            .Items.Count.Should().Be(0);

        (await graphClient.Execute("select [*];", NullScopeContext.Instance))
            .ThrowOnError().Return()
            .Items.Count.Should().Be(0);

        _output.WriteLine($"Retry count={retryCount}");
    }

    private async Task AddNodes(IGraphClient testClient, int index)
    {
        string key = $"node{index}";
        string tags = $"t1,t2=v{index}";

        string cmd = $"add node key={key}, {tags};";
        var option = await testClient.ExecuteBatch(cmd, NullScopeContext.Instance);
        option.IsOk().Should().BeTrue();
    }

    private async Task<Option> AddEdges(IGraphClient testClient, int index)
    {
        string fromKey = $"node{index}";
        string toKey = $"node{index + 1}";
        string tags = $"t1,t2=v{index}";

        string cmd = $"add edge fromKey={fromKey}, toKey={toKey}, {tags};";
        var option = await testClient.ExecuteBatch(cmd, NullScopeContext.Instance);
        return option.ToOptionStatus();
    }

    private async Task<Option> AddBatch(IGraphClient testClient, int index, Func<int, bool> addEdge)
    {
        var cmds = new Sequence<string>();

        string key = $"node{index}";
        string tags = $"t1,t2=v{index}";
        string cmd = $"add node key={key}, {tags};";
        cmds += cmd;

        if (addEdge(index))
        {
            string fromKey = $"node{index - 1}";
            string toKey = $"node{index}";
            string cmd2 = $"add edge fromKey={fromKey}, toKey={toKey}, {tags};";
            cmds += cmd2;
        }

        string command = cmds.Join();
        var option = await testClient.ExecuteBatch(command, NullScopeContext.Instance);
        return option.ToOptionStatus();
    }
}
