using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Graph.Query;

public class GraphDirectoryTests
{
    private GraphMap GetMap()
    {
        string mapJson = """
                        {
              "nodes": [
                {
                  "key": "system:schedule-work",
                  "tags": {},
                  "createdDate": "2023-11-12T20:11:14.6712547Z"
                },
                {
                  "key": "schedulework:WKID-77f4e612-17b4-4ca9-b4eb-93d28bc722be",
                  "tags": {},
                  "createdDate": "2023-11-12T20:11:14.6747667Z"
                },
                {
                  "key": "schedulework:WKID-ee40b722-9041-4527-a38a-542165f43129",
                  "tags": {},
                  "createdDate": "2023-11-12T20:13:55.2607086Z"
                }
              ],
              "edges": [
                {
                  "key": "07785fa8-b01e-4e90-8af8-149d224fd039",
                  "fromKey": "system:schedule-work",
                  "toKey": "schedulework:WKID-77f4e612-17b4-4ca9-b4eb-93d28bc722be",
                  "edgeType": "scheduleWorkType:Active",
                  "tags": {},
                  "createdDate": "2023-11-12T20:11:14.6750191Z"
                },
                {
                  "key": "37ca7c61-f752-41a1-9be2-7d75034b4101",
                  "fromKey": "system:schedule-work",
                  "toKey": "schedulework:WKID-ee40b722-9041-4527-a38a-542165f43129",
                  "edgeType": "scheduleWorkType:Active",
                  "tags": {},
                  "createdDate": "2023-11-12T20:13:55.2607119Z"
                }
              ]
            }
            """;

        var map = GraphMap.FromJson(mapJson);
        map.Should().NotBeNull();

        return map;
    }


    [Fact]
    public void DirectorySearchQuery()
    {
        var map = GetMap();
        var search = map.ExecuteScalar("select [fromKey=system:schedule-work, edgeType=scheduleWorkType:*];", NullScopeContext.Instance);
        search.Should().NotBeNull();
        search.Items.Count.Should().Be(2);
        search.Edges().Count.Should().Be(2);

        var index = search.Items.ToCursor();

        index.NextValue().Return().Cast<GraphEdge>().Action(x =>
        {
            x.Key.ToString().Should().Be("07785fa8-b01e-4e90-8af8-149d224fd039");
            x.FromKey.Should().Be("system:schedule-work");
            x.ToKey.Should().Be("schedulework:WKID-77f4e612-17b4-4ca9-b4eb-93d28bc722be");
            x.EdgeType.Should().Be("scheduleWorkType:Active");
            x.Tags.Count.Should().Be(0);
        });

        index.NextValue().Return().Cast<GraphEdge>().Action(x =>
        {
            x.Key.ToString().Should().Be("37ca7c61-f752-41a1-9be2-7d75034b4101");
            x.FromKey.Should().Be("system:schedule-work");
            x.ToKey.Should().Be("schedulework:WKID-ee40b722-9041-4527-a38a-542165f43129");
            x.EdgeType.Should().Be("scheduleWorkType:Active");
            x.Tags.Count.Should().Be(0);
        });
    }

    [Fact]
    public void UpdateDirectoryEdge()
    {
        var map = GetMap();

        var search = "[fromKey=system:schedule-work, toKey=schedulework:WKID-ee40b722-9041-4527-a38a-542165f43129, edgeType=scheduleWorkType:*]";

        var command = new Sequence<string>()
            .Add("update")
            .Add(search)
            .Add("set edgeType=scheduleWorkType:Completed")
            .Join(" ") + ";";

        var result = map.Execute(command, NullScopeContext.Instance);
        result.Should().NotBeNull();
        result.IsOk().Should().BeTrue(result.ToString());

        GraphQueryResults searchResult = result.Return();
        searchResult.Items.Count.Should().Be(1);
        searchResult.Items[0].Should().NotBeNull();
        searchResult.Items[0].NotNull().Items.Count.Should().Be(1);
        searchResult.Items[0].NotNull().Edges().Count.Should().Be(1);
        searchResult.Items[0].NotNull().Edges()[0].Action(x =>
        {
            x.FromKey.Should().Be("system:schedule-work");
            x.ToKey.Should().Be("schedulework:WKID-ee40b722-9041-4527-a38a-542165f43129");
            x.EdgeType.Should().Be("scheduleWorkType:Active");
        });

        Option<GraphQueryResults> rOption = map.Execute($"select {search};", NullScopeContext.Instance);
        rOption.IsOk().Should().BeTrue();

        GraphQueryResults r = rOption.Return();
        r.Items.Count.Should().Be(1);
        r.Items[0].Should().NotBeNull();
        r.Items[0].NotNull().Items.Count.Should().Be(1);
        r.Items[0].NotNull().Edges().Count.Should().Be(1);
        r.Items[0].NotNull().Edges()[0].Action(x =>
        {
            x.FromKey.Should().Be("system:schedule-work");
            x.ToKey.Should().Be("schedulework:WKID-ee40b722-9041-4527-a38a-542165f43129");
            x.EdgeType.Should().Be("scheduleWorkType:Completed");
        });
    }
}
