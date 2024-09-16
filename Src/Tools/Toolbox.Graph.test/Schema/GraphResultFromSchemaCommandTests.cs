using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Schema;

public class GraphResultFromSchemaCommandTests
{
    [Fact]
    public async Task AddSimpleNode()
    {
        var graph = new GraphMap();
        var testClient = GraphTestStartup.CreateGraphTestHost(graph);

        // Create
        var d = new Data { Key = "key1" };
        var cmds = Data.Schema.Code(d).BuildSetCommands().Join(Environment.NewLine);

        var newMapOption = await testClient.ExecuteBatch(cmds, NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        graph.Nodes.Count.Should().Be(1);
        graph.Nodes.First().Action(x =>
        {
            x.Key.Should().Be("data:key1");
            x.DataMap.Count.Should().Be(1);
            x.DataMap.Values.First().Name.Should().Be("entity");
        });
        graph.Edges.Count.Should().Be(0);


        // Delete
        cmds = Data.Schema.Code(d).BuildDeleteCommands().Join(Environment.NewLine);

        newMapOption = await testClient.ExecuteBatch(cmds, NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        graph.Nodes.Count.Should().Be(0);
        graph.Edges.Count.Should().Be(0);
    }

    [Fact]
    public async Task AddNameIndexNode()
    {
        var graph = new GraphMap();
        var testClient = GraphTestStartup.CreateGraphTestHost(graph);

        // Create
        var d = new Data { Key = "key1", Name = "name1" };
        var cmds = Data.Schema.Code(d).BuildSetCommands().Join(Environment.NewLine);

        var newMapOption = await testClient.ExecuteBatch(cmds, NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        graph.Nodes.Count.Should().Be(2);
        graph.Nodes.ContainsKey("data:key1").Should().BeTrue();
        graph.Nodes["data:key1"].DataMap.First().Value.Name.Should().Be("entity");
        graph.Nodes.ContainsKey("index:name1").Should().BeTrue();
        graph.Nodes["index:name1"].DataMap.Count.Should().Be(0);

        graph.Edges.Count.Should().Be(1);
        graph.Edges.Where(x => x.FromKey == "index:name1" && x.ToKey == "data:key1").Should().HaveCount(1);


        // Update
        var d2 = new Data { Key = "key1", Name = "name2" };
        cmds = Data.Schema.Code(d2).SetCurrent(d).BuildSetCommands().Join(Environment.NewLine);

        newMapOption = await testClient.ExecuteBatch(cmds, NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        graph.Nodes.Count.Should().Be(2);
        graph.Nodes.ContainsKey("data:key1").Should().BeTrue();
        graph.Nodes["data:key1"].DataMap.First().Value.Name.Should().Be("entity");
        graph.Nodes.ContainsKey("index:name2").Should().BeTrue();
        graph.Nodes["index:name2"].DataMap.Count.Should().Be(0);

        graph.Edges.Count.Should().Be(1);
        graph.Edges.Where(x => x.FromKey == "index:name2" && x.ToKey == "data:key1").Should().HaveCount(1);


        // Delete
        cmds = Data.Schema.Code(d2).BuildDeleteCommands().Join(Environment.NewLine);

        newMapOption = await testClient.ExecuteBatch(cmds, NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        graph.Nodes.Count.Should().Be(0);
        graph.Edges.Count.Should().Be(0);
    }

    [Fact]
    public async Task AddProviderIndexNode()
    {
        var graph = new GraphMap();
        var testClient = GraphTestStartup.CreateGraphTestHost(graph);

        // Create
        var d = new Data { Key = "key1", Provider = "provider1", ProviderKey = "provider1-key" };
        var cmds = Data.Schema.Code(d).BuildSetCommands().Join(Environment.NewLine);

        var newMapOption = await testClient.ExecuteBatch(cmds, NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        graph.Nodes.Count.Should().Be(2);
        graph.Nodes.ContainsKey("data:key1").Should().BeTrue();
        graph.Nodes["data:key1"].DataMap.First().Value.Name.Should().Be("entity");
        graph.Nodes.ContainsKey("provider:provider1/provider1-key").Should().BeTrue();
        graph.Nodes["provider:provider1/provider1-key"].DataMap.Count.Should().Be(0);

        graph.Edges.Count.Should().Be(1);
        graph.Edges.Where(x => x.FromKey == "provider:provider1/provider1-key" && x.ToKey == "data:key1").Should().HaveCount(1);

        // Update
        var d2 = new Data { Key = "key1", Provider = "provider1", ProviderKey = "provider1-key2" };
        cmds = Data.Schema.Code(d2).SetCurrent(d).BuildSetCommands().Join(Environment.NewLine);

        newMapOption = await testClient.ExecuteBatch(cmds, NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        graph.Nodes.Count.Should().Be(2);
        graph.Nodes.ContainsKey("data:key1").Should().BeTrue();
        graph.Nodes["data:key1"].DataMap.First().Value.Name.Should().Be("entity");
        graph.Nodes.ContainsKey("provider:provider1/provider1-key2").Should().BeTrue();
        graph.Nodes["provider:provider1/provider1-key2"].DataMap.Count.Should().Be(0);

        graph.Edges.Count.Should().Be(1);
        graph.Edges.Where(x => x.FromKey == "provider:provider1/provider1-key2" && x.ToKey == "data:key1").Should().HaveCount(1);


        // Delete
        cmds = Data.Schema.Code(d).BuildDeleteCommands().Join(Environment.NewLine);

        newMapOption = await testClient.ExecuteBatch(cmds, NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        graph.Nodes.Count.Should().Be(0);
        graph.Edges.Count.Should().Be(0);
    }

    [Fact]
    public async Task AddAgeReferenceIndexNode()
    {
        var graph = new GraphMap();
        var testClient = GraphTestStartup.CreateGraphTestHost(graph);

        var addAgeNodeOption = await testClient.ExecuteBatch("set node key=age:12;", NullScopeContext.Instance);
        addAgeNodeOption.IsOk().Should().BeTrue(addAgeNodeOption.ToString());

        var d = new Data { Key = "key1", Age = 12 };
        var cmds = Data.Schema.Code(d).BuildSetCommands().Join(Environment.NewLine);

        var newMapOption = await testClient.ExecuteBatch(cmds, NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        graph.Nodes.Count.Should().Be(2);
        graph.Nodes.ContainsKey("data:key1").Should().BeTrue();
        graph.Nodes["data:key1"].DataMap.First().Value.Name.Should().Be("entity");
        graph.Nodes.ContainsKey("age:12").Should().BeTrue();
        graph.Nodes["age:12"].DataMap.Count.Should().Be(0);

        graph.Edges.Count.Should().Be(1);
        graph.Edges.Where(x => x.FromKey == "data:key1" && x.ToKey == "age:12").Should().HaveCount(1);

        cmds = Data.Schema.Code(d).BuildDeleteCommands().Join(Environment.NewLine);

        newMapOption = await testClient.ExecuteBatch(cmds, NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        graph.Nodes.Count.Should().Be(1);
        graph.Nodes.ContainsKey("age:12").Should().BeTrue();
        graph.Edges.Count.Should().Be(0);
    }


    private record Data
    {
        public string Key { get; init; } = null!;
        public string? Name { get; init; } = null!;
        public int? Age { get; init; }
        public string? Provider { get; init; }
        public string? ProviderKey { get; init; }

        public static IGraphSchema<Data> Schema { get; } = new GraphSchemaBuilder<Data>()
            .Node(x => x.Key, x => x.IsNotEmpty() ? $"data:{x}" : null)
            .Index(x => x.Name, x => x.IsNotEmpty() ? $"index:{x}" : null)
            .Reference(x => x.Age, x => x > 0 ? $"age:{x}" : null, "ageGroup")
            .Index(x => x.Provider, x => x.ProviderKey, (x, y) => (x, y) switch
            {
                (string provider, string providerKey) => $"provider:{provider}/{providerKey}",
                _ => null
            })
            .Build();
    }

}
