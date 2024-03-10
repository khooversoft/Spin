using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class GraphStressTests
{
    [Fact]
    public async Task SerializeTasks()
    {
        const int count = 1000;
        var map = new GraphMap();

        await AddNodes(map, 0, count);
        await AddEdges(map, 0, count - 1);

        map.Nodes.Count.Should().Be(count);
        map.Edges.Count.Should().Be(count - 1);
    }

    [Fact]
    public async Task ParallelTasks()
    {
        const int count = 1000;
        var map = new GraphMap();

        await AddNodes(map, 0, count);
        map.Nodes.Count.Should().Be(count);

        Task[] tasks = [
            AddNodes(map, count, count),
            AddEdges(map, 0, count - 1),
        ];

        await Task.WhenAll(tasks);

        map.Nodes.Count.Should().Be(count * 2);
        map.Edges.Count.Should().Be(count - 1);
    }

    [Fact]
    public async Task SerializeBatchTasks()
    {
        const int count = 10;
        var map = new GraphMap();

        await AddBatch(map, 0, count);

        map.Nodes.Count.Should().Be(count);
        map.Edges.Count.Should().Be(count - 1);
    }

    [Fact]
    public async Task ParallelBatchTasks()
    {
        const int count = 10;
        const int batchCount = 10;
        var map = new GraphMap();

        var tasks = Enumerable.Range(0, batchCount)
            .Select(x => AddBatch(map, x * count, count))
            .ToArray();

        await Task.WhenAll(tasks);

        map.Nodes.Count.Should().Be(batchCount * count);
        map.Edges.Count.Should().Be((batchCount - 1) * count);
    }

    private Task AddNodes(GraphMap map, int start, int count)
    {
        for (int i = 0; i < count; i++)
        {
            string key = $"node{i + start}";
            string tags = $"t1,t2=v{i + start}";

            string cmd = $"add node key={key}, {tags};";
            var option = map.Execute(cmd, NullScopeContext.Instance);
            option.IsOk().Should().BeTrue();
        }

        return Task.CompletedTask;
    }

    private Task AddEdges(GraphMap map, int start, int count)
    {
        for (int i = 0; i < count; i++)
        {
            string fromKey = $"node{i + start}";
            string toKey = $"node{i + start + 1}";
            string tags = $"t1,t2=v{i + start}";

            string cmd = $"add edge fromKey={fromKey}, toKey={toKey}, {tags};";
            var option = map.Execute(cmd, NullScopeContext.Instance);
            option.IsOk().Should().BeTrue(option.ToString());
        }

        return Task.CompletedTask;
    }

    private Task AddBatch(GraphMap map, int start, int count)
    {
        var cmds = new Sequence<string>();

        for (int i = 0; i < count; i++)
        {
            string key = $"node{i + start}";
            string tags = $"t1,t2=v{i + start}";
            string cmd = $"add node key={key}, {tags};";
            cmds += cmd;

            if (i > 0)
            {
                string fromKey = $"node{i + start - 1}";
                string toKey = $"node{i + start}";
                string cmd2 = $"add edge fromKey={fromKey}, toKey={toKey}, {tags};";
                cmds += cmd2;
            }
        }

        string command = cmds.Join();
        var option = map.Execute(command, NullScopeContext.Instance);
        option.IsOk().Should().BeTrue(option.ToString());
        return Task.CompletedTask;
    }
}
