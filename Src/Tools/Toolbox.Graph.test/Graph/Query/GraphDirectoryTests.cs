using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.Graph.Query;

public class GraphDirectoryTests
{
    private readonly ITestOutputHelper _outputHelper;

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
                  "fromKey": "system:schedule-work",
                  "toKey": "schedulework:WKID-77f4e612-17b4-4ca9-b4eb-93d28bc722be",
                  "edgeType": "scheduleWorkType:Active",
                  "tags": {},
                  "createdDate": "2023-11-12T20:11:14.6750191Z"
                },
                {
                  "fromKey": "system:schedule-work",
                  "toKey": "schedulework:WKID-ee40b722-9041-4527-a38a-542165f43129",
                  "edgeType": "scheduleWorkType:Completed",
                  "tags": {},
                  "createdDate": "2023-11-12T20:13:55.2607119Z"
                }
              ]
            }
            """;

        var map = GraphMapTool.FromJson(mapJson);
        map.NotNull();

        return map;
    }

    public GraphDirectoryTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact]
    public async Task DirectorySearchQuery()
    {
        using GraphHostService graphTestClient = await GraphTestStartup.CreateGraphService(GetMap().Clone(), logOutput: x => _outputHelper.WriteLine(x));
        var context = graphTestClient.CreateScopeContext<GraphDirectoryTests>();

        var search = (await graphTestClient.ExecuteBatch("select [from=system:schedule-work, type=scheduleWorkType:*];", context)).ThrowOnError().Return();
        Assert.NotNull(search);
        search.NotNull();
        search.Items.Count.Be(1);
        search.Items[0].Edges.Count.Be(2);

        var index = search.Items[0].Edges.OrderBy(x => x.ToKey).ToArray().ToCursor();

        index.NextValue().Return().Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Be("system:schedule-work");
            x.ToKey.Be("schedulework:WKID-77f4e612-17b4-4ca9-b4eb-93d28bc722be");
            x.EdgeType.Be("scheduleWorkType:Active");
            x.Tags.Count.Be(0);
        });

        index.NextValue().Return().Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Be("system:schedule-work");
            x.ToKey.Be("schedulework:WKID-ee40b722-9041-4527-a38a-542165f43129");
            x.EdgeType.Be("scheduleWorkType:Completed");
            x.Tags.Count.Be(0);
        });
    }

    //[Fact]
    //public async Task UpdateDirectoryEdge()
    //{
    //    var map = GetMap();
    //    var testClient = GraphTestStartup.CreateGraphTestHost(map);

    //    var search = "[fromKey=system:schedule-work, toKey=schedulework:WKID-ee40b722-9041-4527-a38a-542165f43129, edgeType=scheduleWorkType:*]";

    //    var command = new Sequence<string>()
    //        .Add("set")
    //        .Add(search)
    //        .Add("set edgeType=scheduleWorkType:Completed")
    //        .Join(" ") + ";";

    //    var result = await testClient.ExecuteBatch(command, NullScopeContext.Instance);
    //    result.NotBeNull();
    //    result.IsOk().BeTrue(result.ToString());

    //    QueryBatchResult searchResult = result.Return();
    //    searchResult.Items.Count.Be(1);
    //    searchResult.Items[0].Action(r =>
    //    {
    //        r.NotBeNull();
    //        r.Nodes.Count.Be(1);
    //        r.Edges.Action(x =>
    //        {
    //            x.FromKey.Be("system:schedule-work");
    //            x.ToKey.Be("schedulework:WKID-ee40b722-9041-4527-a38a-542165f43129");
    //            x.EdgeType.Be("scheduleWorkType:Active");
    //        });
    //    });


    //    Option<GraphQueryResults> rOption = await testClient.ExecuteBatch($"select {search};", NullScopeContext.Instance);
    //    rOption.IsOk().BeTrue();

    //    GraphQueryResults r = rOption.Return();
    //    r.Items.Length.Be(1);
    //    r.Items[0].NotBeNull();
    //    r.Items[0].NotNull().Items.Count.Be(1);
    //    r.Items[0].NotNull().Edges().Length.Be(1);
    //    r.Items[0].NotNull().Edges()[0].Action(x =>
    //    {
    //        x.FromKey.Be("system:schedule-work");
    //        x.ToKey.Be("schedulework:WKID-ee40b722-9041-4527-a38a-542165f43129");
    //        x.EdgeType.Be("scheduleWorkType:Completed");
    //    });
    //}
}
