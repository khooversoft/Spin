//using FluentAssertions;
//using Microsoft.Extensions.DependencyInjection;
//using Toolbox.Extensions;
//using Toolbox.Types;

//namespace Toolbox.Graph.test.Command;

//public class GraphSimpleTaskStressTests2
//{
//    [Fact]
//    public async Task SerializeTasks()
//    {
//        const int count = 1000;
//        var testClient = GraphTestStartup.CreateGraphTestHost();
//        var map = testClient.ServiceProvider.GetRequiredService<GraphMap>();

//        await AddNodes(testClient, 0, count);
//        await AddEdges(testClient, 0, count - 1);

//        map.Nodes.Count.Should().Be(count);
//        map.Edges.Count.Should().Be(count - 1);
//    }

//    [Fact]
//    public async Task ParallelTasks()
//    {
//        const int count = 1000;
//        var testClient = GraphTestStartup.CreateGraphTestHost();
//        var map = testClient.ServiceProvider.GetRequiredService<GraphMap>();

//        await AddNodes(testClient, 0, count);
//        map.Nodes.Count.Should().Be(count);

//        Task[] tasks = [
//            AddNodes(testClient, count, count),
//            AddEdges(testClient, 0, count - 1),
//        ];

//        await Task.WhenAll(tasks);

//        map.Nodes.Count.Should().Be(count * 2);
//        map.Edges.Count.Should().Be(count - 1);
//    }

//    [Fact]
//    public async Task SerializeBatchTasks()
//    {
//        const int count = 10;
//        var testClient = GraphTestStartup.CreateGraphTestHost();
//        var map = testClient.ServiceProvider.GetRequiredService<GraphMap>();

//        await AddBatch(testClient, 0, count);

//        map.Nodes.Count.Should().Be(count);
//        map.Edges.Count.Should().Be(count - 1);
//    }

//    [Theory]
//    [InlineData(10, 10)]
//    [InlineData(10, 100)]
//    [InlineData(10, 1000)]
//    public async Task ParallelBatchTasks(int batchCount, int count)
//    {
//        var testClient = GraphTestStartup.CreateGraphTestHost();
//        var map = testClient.ServiceProvider.GetRequiredService<GraphMap>();

//        var tasks = Enumerable.Range(0, batchCount)
//            .Select(x => AddBatch(testClient, x * count, count))
//            .ToArray();

//        await Task.WhenAll(tasks);

//        map.Nodes.Count.Should().Be(batchCount * count);
//        map.Edges.Count.Should().Be((batchCount * (count - 1)));
//    }

//    private async Task AddNodes(GraphTestClient testClient, int start, int count)
//    {
//        for (int i = 0; i < count; i++)
//        {
//            string key = $"node{i + start}";
//            string tags = $"t1,t2=v{i + start}";

//            string cmd = $"add node key={key}, {tags};";
//            var option = await testClient.ExecuteBatch(cmd, NullScopeContext.Instance);
//            option.IsOk().Should().BeTrue();
//        }
//    }

//    private async Task AddEdges(GraphTestClient testClient, int start, int count)
//    {
//        for (int i = 0; i < count; i++)
//        {
//            string fromKey = $"node{i + start}";
//            string toKey = $"node{i + start + 1}";
//            string tags = $"t1,t2=v{i + start}";

//            string cmd = $"add edge fromKey={fromKey}, toKey={toKey}, {tags};";
//            var option = await testClient.ExecuteBatch(cmd, NullScopeContext.Instance);
//            option.IsOk().Should().BeTrue(option.ToString());
//        }
//    }

//    private async Task AddBatch(GraphTestClient testClient, int start, int count)
//    {
//        var cmds = new Sequence<string>();

//        for (int i = 0; i < count; i++)
//        {
//            string key = $"node{i + start}";
//            string tags = $"t1,t2=v{i + start}";
//            string cmd = $"add node key={key}, {tags};";
//            cmds += cmd;

//            if (i > 0)
//            {
//                string fromKey = $"node{i + start - 1}";
//                string toKey = $"node{i + start}";
//                string cmd2 = $"add edge fromKey={fromKey}, toKey={toKey}, {tags};";
//                cmds += cmd2;
//            }
//        }

//        string command = cmds.Join();
//        var option = await testClient.ExecuteBatch(command, NullScopeContext.Instance);
//        option.IsOk().Should().BeTrue(option.ToString());
//    }
//}
