using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class GraphParallelStressTests
{
    [Fact]
    public async Task ParallelAddTasks()
    {
        const int count = 1000;
        var testClient = GraphTestStartup.CreateGraphTestHost();
        GraphMap map = testClient.ServiceProvider.GetRequiredService<IGraphContext>().Map;

        await ActionParallel.Run(x => AddNodes(testClient, x), Enumerable.Range(0, count), 5);
        await ActionParallel.Run(x => AddEdges(testClient, x), Enumerable.Range(0, count - 1), 5);

        map.Nodes.Count.Should().Be(count);
        map.Edges.Count.Should().Be(count - 1);
    }

    private async Task AddNodes(GraphTestClient testClient, int index)
    {
        string key = $"node{index}";
        string tags = $"t1,t2=v{index}";

        string cmd = $"set node key={key} set {tags};";
        var option = await testClient.ExecuteBatch(cmd, NullScopeContext.Instance);
        option.IsOk().Should().BeTrue();
    }

    private async Task<Option> AddEdges(GraphTestClient testClient, int index)
    {
        string fromKey = $"node{index}";
        string toKey = $"node{index + 1}";
        string tags = $"t1,t2=v{index}";

        string cmd = $"set edge from={fromKey}, to={toKey}, type=et set {tags};";
        var option = await testClient.ExecuteBatch(cmd, NullScopeContext.Instance);
        option.IsOk().Should().BeTrue(option.ToString());
        return option.ToOptionStatus();
    }

    private async Task<Option> AddBatch(GraphTestClient testClient, int index, Func<int, bool> addEdge)
    {
        var cmds = new Sequence<string>();

        string key = $"node{index}";
        string tags = $"t1,t2=v{index}";
        string cmd = $"set node key={key}, {tags};";
        cmds += cmd;

        if (addEdge(index))
        {
            string fromKey = $"node{index - 1}";
            string toKey = $"node{index}";
            string cmd2 = $"set edge Key={fromKey}, toKey={toKey}, type=et set {tags};";
            cmds += cmd2;
        }

        string command = cmds.Join();
        var option = await testClient.ExecuteBatch(command, NullScopeContext.Instance);
        option.IsOk().Should().BeTrue();
        return option.ToOptionStatus();
    }
}
