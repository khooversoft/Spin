using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Schema;

public class MultipleDataOnNodes
{
    [Fact]
    public async Task TwoDataOnSingleNode()
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
            x.DataMap.First().Value.Name.Should().Be("entity");
        });
        graph.Edges.Count.Should().Be(0);

        var readOption = await testClient.ExecuteBatch("select (key=data:key1) return entity;", NullScopeContext.Instance);
        readOption.IsOk().Should().BeTrue();
        readOption.Return().Items.Length.Should().Be(1);
        var d_read = readOption.Return().Items.First().ReturnNames["entity"].ToObject<Data>();
        (d == d_read).Should().BeTrue();

        // Attached data
        var w = new Weather { Key = "key1" };
        cmds = Weather.Schema.Code(w).BuildSetCommands().Join(Environment.NewLine);
        newMapOption = await testClient.ExecuteBatch(cmds, NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        graph.Nodes.Count.Should().Be(1);
        graph.Nodes.First().Action(x =>
        {
            x.Key.Should().Be("data:key1");
            x.DataMap.Count.Should().Be(2);
            Enumerable.SequenceEqual(x.DataMap.Values.Select(x => x.Name).OrderBy(x => x), ["entity", "weather"]).Should().BeTrue();
        });
        graph.Edges.Count.Should().Be(0);

        readOption = await testClient.ExecuteBatch("select (key=data:key1) return entity;", NullScopeContext.Instance);
        readOption.IsOk().Should().BeTrue();
        readOption.Return().Items.Length.Should().Be(1);
        d_read = readOption.Return().Items.First().ReturnNames["entity"].ToObject<Data>();
        (d == d_read).Should().BeTrue();

        readOption = await testClient.ExecuteBatch("select (key=data:key1) return weather;", NullScopeContext.Instance);
        readOption.IsOk().Should().BeTrue();
        readOption.Return().Items.Length.Should().Be(1);
        var w_read = readOption.Return().Items.First().ReturnNames["weather"].ToObject<Weather>();
        (w == w_read).Should().BeTrue();

        readOption = await testClient.ExecuteBatch("select (key=data:key1) return weather, data;", NullScopeContext.Instance);
        readOption.IsOk().Should().BeTrue();
        readOption.Return().Action(x =>
        {
            x.Items.Length.Should().Be(1);
            x.Items.First().Action(y =>
            {
                y.ReturnNames.Count.Should().Be(2);
                var r1 = y.ReturnNames["entity"].ToObject<Data>();
                var r2 = y.ReturnNames["weather"].ToObject<Weather>();
                (d == r1).Should().BeTrue();
                (w == r2).Should().BeTrue();
            });
        });


        // Delete
        cmds = Data.Schema.Code(d).BuildDeleteCommands().Join(Environment.NewLine);

        newMapOption = await testClient.ExecuteBatch(cmds, NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        graph.Nodes.Count.Should().Be(0);
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

    private record Weather
    {
        public string Key { get; init; } = null!;
        public string City { get; init; } = null!;
        public int Temp { get; init; }
        public static IGraphSchema<Weather> Schema { get; } = new GraphSchemaBuilder<Weather>()
            .DataName("weather")
            .Node(x => x.Key, x => x.IsNotEmpty() ? $"data:{x}" : null)
            .Build();
    }
}
