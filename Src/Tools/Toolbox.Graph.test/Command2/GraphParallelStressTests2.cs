//using System.Threading.Tasks.Dataflow;
//using FluentAssertions;
//using Microsoft.Extensions.DependencyInjection;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Graph.test.Command;

//public class GraphParallelStressTests2
//{
//    [Fact]
//    public async Task SerializeTasks()
//    {
//        const int count = 10000;
//        var testClient = GraphTestStartup.CreateGraphTestHost();
//        var map = testClient.ServiceProvider.GetRequiredService<GraphMap>();

//        await ActionParallel.Run(x => AddNodes(testClient, x), Enumerable.Range(0, count), 5);
//        await ActionParallel.Run(x => AddEdges(testClient, x), Enumerable.Range(0, count - 1), 5);

//        map.Nodes.Count.Should().Be(count);
//        map.Edges.Count.Should().Be(count - 1);
//    }

//    [Fact]
//    public async Task ParallelTasks()
//    {
//        const int count = 10000;
//        var blockConfig = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 };
//        var testClient = GraphTestStartup.CreateGraphTestHost();
//        var map = testClient.ServiceProvider.GetRequiredService<GraphMap>();
//        int sucessCount = 0;
//        ManualResetEventSlim resetEvent = new ManualResetEventSlim();

//        ActionBlock<int> edgeBlock = null!;
//        edgeBlock = new ActionBlock<int>(async x =>
//        {
//            var option = await AddEdges(testClient, x);
//            if (option.IsError())
//            {
//                await edgeBlock.SendAsync(x);
//                return;
//            }

//            int current = Interlocked.Increment(ref sucessCount);
//            if (current == count - 1)
//            {
//                resetEvent.Set();
//            }
//        }, blockConfig);

//        var nodeBlock = new ActionBlock<int>(async x =>
//        {
//            await AddNodes(testClient, x);
//            await edgeBlock.SendAsync(x);
//        }, blockConfig);

//        Enumerable.Range(0, count).ForEach(x => nodeBlock.Post(x).Should().BeTrue());

//        resetEvent.Wait();
//        nodeBlock.Complete();
//        await nodeBlock.Completion;

//        edgeBlock.Complete();
//        await edgeBlock.Completion;

//        map.Nodes.Count.Should().Be(count);
//        map.Edges.Count.Should().Be(count - 1);
//    }

//    [Fact]
//    public async Task SerializeBatchTasksWithErrorRecovery()
//    {
//        const int count = 100;
//        var blockConfig = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 };
//        var testClient = GraphTestStartup.CreateGraphTestHost();
//        var map = testClient.ServiceProvider.GetRequiredService<GraphMap>();
//        int errorCount = 0;
//        int sucessCount = 0;
//        ManualResetEventSlim resetEvent = new ManualResetEventSlim();

//        ActionBlock<int> batchBlock = null!;
//        batchBlock = new ActionBlock<int>(async x =>
//        {
//            var option = await AddBatch(testClient, x, x => x > 0);
//            if (option.IsError())
//            {
//                Interlocked.Increment(ref errorCount);
//                batchBlock.Post(x);
//                return;
//            }

//            int current = Interlocked.Increment(ref sucessCount);
//            if (current == count - 1)
//            {
//                resetEvent.Set();
//            }
//        }, blockConfig);

//        Enumerable.Range(0, count).ForEach(x => batchBlock.Post(x).Should().BeTrue());

//        resetEvent.Wait();
//        batchBlock.Complete();
//        await batchBlock.Completion;

//        map.Nodes.Count.Should().Be(count);
//        map.Edges.Count.Should().Be(count - 1);
//        errorCount.Should().BeGreaterThan(0);
//    }

//    private async Task AddNodes(GraphTestClient testClient, int index)
//    {
//        string key = $"node{index}";
//        string tags = $"t1,t2=v{index}";

//        string cmd = $"add node key={key}, {tags};";
//        var option = await testClient.ExecuteBatch(cmd, NullScopeContext.Instance);
//        option.IsOk().Should().BeTrue();
//    }

//    private async Task<Option> AddEdges(GraphTestClient testClient, int index)
//    {
//        string fromKey = $"node{index}";
//        string toKey = $"node{index + 1}";
//        string tags = $"t1,t2=v{index}";

//        string cmd = $"add edge fromKey={fromKey}, toKey={toKey}, {tags};";
//        var option = await testClient.ExecuteBatch(cmd, NullScopeContext.Instance);
//        return option.ToOptionStatus();
//    }

//    private async Task<Option> AddBatch(GraphTestClient testClient, int index, Func<int, bool> addEdge)
//    {
//        var cmds = new Sequence<string>();

//        string key = $"node{index}";
//        string tags = $"t1,t2=v{index}";
//        string cmd = $"add node key={key}, {tags};";
//        cmds += cmd;

//        if (addEdge(index))
//        {
//            string fromKey = $"node{index - 1}";
//            string toKey = $"node{index}";
//            string cmd2 = $"add edge fromKey={fromKey}, toKey={toKey}, {tags};";
//            cmds += cmd2;
//        }

//        string command = cmds.Join();
//        var option = await testClient.ExecuteBatch(command, NullScopeContext.Instance);
//        return option.ToOptionStatus();
//    }
//}
