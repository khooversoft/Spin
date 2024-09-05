﻿using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class GraphUpsertCommandNodeTests
{
    private readonly GraphMap _map = new GraphMap()
    {
        new GraphNode("node1", tags: "name=marko,age=29"),
        new GraphNode("node2", tags: "name=vadas,age=27"),
        new GraphNode("node3", tags: "name=lop,lang=java"),
        new GraphNode("node4", tags: "name=josh,age=32"),
        new GraphNode("node5", tags: "name=ripple,lang=java"),
        new GraphNode("node6", tags: "name=peter,age=35"),
        new GraphNode("node7", tags: "lang=java"),

        new GraphEdge("node1", "node2", tags: "knows,level=1"),
        new GraphEdge("node1", "node3", tags: "knows,level=1"),
        new GraphEdge("node6", "node3", tags: "created"),
        new GraphEdge("node4", "node5", tags: "created"),
        new GraphEdge("node4", "node3", tags: "created"),
    };

    [Fact]
    public async Task UpsertForNode()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("upsert node key=node1, newTags;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node1");
            x.Tags.ToTagsString().Should().Be("age=29,name=marko,newTags");
        });

        commandResults.Items.Length.Should().Be(1);
        Cursor<GraphQueryResult> resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.AddNode);
            x.Status.IsOk().Should().BeTrue();
            x.Items.Should().NotBeNull();
        });

        var lookupOption = await testClient.ExecuteBatch("select (key=node1);", NullScopeContext.Instance);
        lookupOption.IsOk().Should().BeTrue(newMapOption.ToString());

        GraphQueryResults lookupResults = lookupOption.Return();
        lookupResults.Items.Length.Should().Be(1);
        lookupResults.Get<GraphNode>().First().Action(x =>
        {
            x.Key.Should().Be("node1");
            x.Tags.ToTagsString().Should().Be("age=29,name=marko,newTags");
        });
    }

    [Fact]
    public async Task UpsertForNodeWithTagValue()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("upsert node key=node1, newTags=v99;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node1");
            x.Tags.ToTagsString().Should().Be("age=29,name=marko,newTags=v99");
        });

        commandResults.Items.Length.Should().Be(1);
        Cursor<GraphQueryResult> resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.AddNode);
            x.Status.IsOk().Should().BeTrue();
            x.Items.Should().NotBeNull();
        });

        var lookupOption = await testClient.ExecuteBatch("select (key=node1);", NullScopeContext.Instance);
        lookupOption.IsOk().Should().BeTrue(newMapOption.ToString());

        GraphQueryResults lookupResults = lookupOption.Return();
        lookupResults.Items.Length.Should().Be(1);
        lookupResults.Get<GraphNode>().First().Action(x =>
        {
            x.Key.Should().Be("node1");
            x.Tags.ToTagsString().Should().Be("age=29,name=marko,newTags=v99");
        });
    }

    [Fact]
    public async Task SingleUpdateForNodeWithData()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("upsert node key=node3, contract { 'aGVsbG8=' };", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        var index = compareMap.ToCursor();

        index.NextValue().Return().Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node3");
            x.Tags.ToTagsString().Should().Be("lang=java,name=lop");
        });

        commandResults.Items.Length.Should().Be(1);

        GraphQueryResult search = (await testClient.Execute("select (key=node3);", NullScopeContext.Instance)).ThrowOnError().Return();
        search.Status.IsOk().Should().BeTrue();
        search.Items.Count.Should().Be(1);
        search.Items[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node3");
            x.Tags.Count.Should().Be(2);
            x.DataMap.Count.Should().Be(1);
            x.DataMap.Values.First().Action(y =>
            {
                y.FileId.Should().Be("nodes/node3/node3___contract.json");
            });
        });
    }

    [Fact]
    public async Task UpsertForNodeWithRemoveTag()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("upsert node key=node6, -name;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node6");
            x.Tags.ToTagsString().Should().Be("age=35");
        });

        commandResults.Items.Length.Should().Be(1);
        Cursor<GraphQueryResult> resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.AddNode);
            x.Status.IsOk().Should().BeTrue();
            x.Items.Should().NotBeNull();
        });

        var lookupOption = await testClient.ExecuteBatch("select (key=node6);", NullScopeContext.Instance);
        lookupOption.IsOk().Should().BeTrue(newMapOption.ToString());

        GraphQueryResults lookupResults = lookupOption.Return();
        lookupResults.Items.Length.Should().Be(1);
        lookupResults.Get<GraphNode>().First().Action(x =>
        {
            x.Key.Should().Be("node6");
            x.Tags.ToTagsString().Should().Be("age=35");
        });
    }

    [Fact]
    public async Task SingleAddWithUpsertForNode()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("upsert node key=node99, newTags;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node99");
            x.Tags.ToTagsString().Should().Be("newTags");
        });

        commandResults.Items.Length.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.AddNode);
            x.Status.IsOk().Should().BeTrue();
            x.Items.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task SingleAddWithUpsertForNodeWithMultipleTags()
    {
        var copyMap = _map.Clone();
        var testClient = GraphTestStartup.CreateGraphTestHost(copyMap);
        var newMapOption = await testClient.ExecuteBatch("upsert node key=node99, newTags,label=client;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(_map, copyMap);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node99");
            x.Tags.ToTagsString().Should().Be("label=client,newTags");
        });

        commandResults.Items.Length.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.AddNode);
            x.Status.IsOk().Should().BeTrue();
            x.Items.Should().NotBeNull();
        });
    }
}
