using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Schema;

public class GraphSchemaMultipleReferencesTests
{
    [Fact]
    public void KeyValue()
    {
        var d = new Data
        {
            Key = "key1",
            Name = "name1",
            OwnerReference = "owner@domain.com",
            Roles = [
                new RoleRecord { PrincipalId = "role1" },
                new RoleRecord { PrincipalId = "role2" },
            ],
        };

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Node).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(1);
            x[0].Should().Be("data:key1");
        });

        Data.Schema.SchemaValues.GetValues(d, SchemaType.Reference).Action(x =>
        {
            x.Should().NotBeNull();
            x.Count.Should().Be(3);
            string[] compareTo = ["data-to-role`role:owner@domain.com", "data-to-role`role:role1", "data-to-role`role:role2"];
            Enumerable.SequenceEqual(x.OrderBy(x => x), compareTo).Should().BeTrue();
        });
    }

    [Fact]
    public async Task SetCommand()
    {
        var graph = new GraphMap();
        var testClient = GraphTestStartup.CreateGraphTestHost(graph);

        // Create
        var d = new Data
        {
            Key = "key1",
            Name = "name1",
            OwnerReference = "owner@domain.com",
            Roles = [
                new RoleRecord { PrincipalId = "role1" },
                new RoleRecord { PrincipalId = "role2" },
            ],
        };

        (await testClient.ExecuteBatch(new string[] {
            "set node key=role:owner@domain.com;",
            "set node key=role:role1;",
            "set node key=role:role2;",
            "set node key=role:owner@domain.com-2;",
            "set node key=role:role1-2;",
            "set node key=role:role2-2;",
        }.Join(Environment.NewLine), NullScopeContext.Instance))
        .Action(x => x.IsOk().Should().BeTrue(x.ToString()));

        const int _baseNodeCount = 6;

        graph.Nodes.Action(x =>
        {
            x.Count.Should().Be(_baseNodeCount);
            x.ContainsKey("role:owner@domain.com").Should().BeTrue();
            x.ContainsKey("role:role1").Should().BeTrue();
            x.ContainsKey("role:role2").Should().BeTrue();
            x.ContainsKey("role:owner@domain.com-2").Should().BeTrue();
            x.ContainsKey("role:role1-2").Should().BeTrue();
            x.ContainsKey("role:role2-2").Should().BeTrue();
        });
        graph.Edges.Count.Should().Be(0);

        var cmds = Data.Schema.Code(d).BuildSetCommands().Join(Environment.NewLine);

        string[] expectedCmds = [
            "set node key=data:key1 set entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6Im5hbWUxIiwib3duZXJSZWZlcmVuY2UiOiJvd25lckBkb21haW4uY29tIiwicm9sZXMiOlt7InByaW5jaXBhbElkIjoicm9sZTEifSx7InByaW5jaXBhbElkIjoicm9sZTIifV19' };",
            "set edge from=data:key1, to=role:owner@domain.com, type=data-to-role;",
            "set edge from=data:key1, to=role:role1, type=data-to-role;",
            "set edge from=data:key1, to=role:role2, type=data-to-role;",
            ];

        cmds.Should().Be(expectedCmds.Join(Environment.NewLine));

        var newMapOption = await testClient.ExecuteBatch(cmds, NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        graph.Nodes.Count.Should().Be(_baseNodeCount + 1);
        graph.Nodes.ContainsKey("data:key1").Should().BeTrue();
        graph.Nodes["data:key1"].DataMap.First().Value.Name.Should().Be("entity");
        graph.Nodes["role:owner@domain.com"].DataMap.Count.Should().Be(0);
        graph.Nodes["role:role1"].DataMap.Count.Should().Be(0);
        graph.Nodes["role:role2"].DataMap.Count.Should().Be(0);

        graph.Edges.Count.Should().Be(3);
        graph.Edges.Where(x => x.FromKey == "data:key1" && x.ToKey == "role:owner@domain.com" && x.EdgeType == "data-to-role").Should().HaveCount(1);
        graph.Edges.Where(x => x.FromKey == "data:key1" && x.ToKey == "role:role1" && x.EdgeType == "data-to-role").Should().HaveCount(1);
        graph.Edges.Where(x => x.FromKey == "data:key1" && x.ToKey == "role:role2" && x.EdgeType == "data-to-role").Should().HaveCount(1);


        // Update
        var d2 = new Data
        {
            Key = "key1",
            Name = "name1",
            OwnerReference = "owner@domain.com-2",
            Roles = [
                new RoleRecord { PrincipalId = "role1-2" },
                new RoleRecord { PrincipalId = "role2-2" },
            ],
        };

        cmds = Data.Schema.Code(d2).SetCurrent(d).BuildSetCommands().Join(Environment.NewLine);

        expectedCmds = [
            "set node key=data:key1 set entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6Im5hbWUxIiwib3duZXJSZWZlcmVuY2UiOiJvd25lckBkb21haW4uY29tLTIiLCJyb2xlcyI6W3sicHJpbmNpcGFsSWQiOiJyb2xlMS0yIn0seyJwcmluY2lwYWxJZCI6InJvbGUyLTIifV19' };",
            "delete edge ifexist from=data:key1, to=role:owner@domain.com, type=data-to-role;",
            "delete edge ifexist from=data:key1, to=role:role1, type=data-to-role;",
            "delete edge ifexist from=data:key1, to=role:role2, type=data-to-role;",
            "set edge from=data:key1, to=role:owner@domain.com-2, type=data-to-role;",
            "set edge from=data:key1, to=role:role1-2, type=data-to-role;",
            "set edge from=data:key1, to=role:role2-2, type=data-to-role;",
            ];

        cmds.Should().Be(expectedCmds.Join(Environment.NewLine));

        newMapOption = await testClient.ExecuteBatch(cmds, NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        graph.Nodes.Count.Should().Be(_baseNodeCount + 1);
        graph.Nodes.ContainsKey("data:key1").Should().BeTrue();
        graph.Nodes["data:key1"].DataMap.First().Value.Name.Should().Be("entity");
        graph.Nodes["role:owner@domain.com-2"].DataMap.Count.Should().Be(0);
        graph.Nodes["role:role1-2"].DataMap.Count.Should().Be(0);
        graph.Nodes["role:role2-2"].DataMap.Count.Should().Be(0);

        graph.Edges.Count.Should().Be(3);
        graph.Edges.Where(x => x.FromKey == "data:key1" && x.ToKey == "role:owner@domain.com-2" && x.EdgeType == "data-to-role").Should().HaveCount(1);
        graph.Edges.Where(x => x.FromKey == "data:key1" && x.ToKey == "role:role1-2" && x.EdgeType == "data-to-role").Should().HaveCount(1);
        graph.Edges.Where(x => x.FromKey == "data:key1" && x.ToKey == "role:role2-2" && x.EdgeType == "data-to-role").Should().HaveCount(1);

        // Delete
        cmds = Data.Schema.Code(d).BuildDeleteCommands().Join(Environment.NewLine);

        newMapOption = await testClient.ExecuteBatch(cmds, NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        graph.Nodes.Count.Should().Be(_baseNodeCount);
        graph.Edges.Count.Should().Be(0);
    }

    private record Data
    {
        public string Key { get; init; } = null!;
        public string Name { get; init; } = null!;
        public string OwnerReference { get; init; } = null!;
        public IReadOnlyList<RoleRecord> Roles { get; init; } = Array.Empty<RoleRecord>();

        public static IGraphSchema<Data> Schema { get; } = new GraphSchemaBuilder<Data>()
            .Node(x => x.Key, x => x.IsNotEmpty() ? $"data:{x}" : null)
            .Reference(x => x.OwnerReference, x => x.IsNotEmpty() ? $"role:{x}" : null, "data-to-role")
            .ReferenceCollection(x => x.Roles.Select(y => y.PrincipalId), x => x.IsNotEmpty() ? $"role:{x}" : null, "data-to-role")
            .Build();
    }

    private record RoleRecord
    {
        public string PrincipalId { get; init; } = null!;
    }
}
