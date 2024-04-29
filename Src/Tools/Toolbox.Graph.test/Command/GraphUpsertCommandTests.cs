﻿using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class GraphUpsertCommandTests
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
    public void UpsertForNode()
    {
        var copyMap = _map.Copy();
        var newMapOption = _map.Execute("upsert node key=node1, newTags;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(copyMap, _map);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node1");
            x.Tags.ToTagsString().Should().Be("age=29,name=marko,newTags");
        });

        commandResults.Items.Count.Should().Be(1);
        Cursor<GraphQueryResult> resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.AddNode);
            x.Status.IsOk().Should().BeTrue();
            x.Items.Should().NotBeNull();
        });

        var lookupOption = _map.Execute("select (key=node1);", NullScopeContext.Instance);
        lookupOption.IsOk().Should().BeTrue(newMapOption.ToString());

        GraphQueryResults lookupResults = lookupOption.Return();
        lookupResults.Items.Count.Should().Be(1);
        lookupResults.Get<GraphNode>().First().Action(x =>
        {
            x.Key.Should().Be("node1");
            x.Tags.ToTagsString().Should().Be("age=29,name=marko,newTags");
        });
    }

    [Fact]
    public void UpsertForNodeWithTagValue()
    {
        var copyMap = _map.Copy();
        var newMapOption = _map.Execute("upsert node key=node1, newTags=v99;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(copyMap, _map);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node1");
            x.Tags.ToTagsString().Should().Be("age=29,name=marko,newTags=v99");
        });

        commandResults.Items.Count.Should().Be(1);
        Cursor<GraphQueryResult> resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.AddNode);
            x.Status.IsOk().Should().BeTrue();
            x.Items.Should().NotBeNull();
        });

        var lookupOption = _map.Execute("select (key=node1);", NullScopeContext.Instance);
        lookupOption.IsOk().Should().BeTrue(newMapOption.ToString());

        GraphQueryResults lookupResults = lookupOption.Return();
        lookupResults.Items.Count.Should().Be(1);
        lookupResults.Get<GraphNode>().First().Action(x =>
        {
            x.Key.Should().Be("node1");
            x.Tags.ToTagsString().Should().Be("age=29,name=marko,newTags=v99");
        });
    }

    [Fact]
    public void UpsertForNodeWithRemoveTag()
    {
        var copyMap = _map.Copy();
        var newMapOption = _map.Execute("upsert node key=node6, -name;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(copyMap, _map);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node6");
            x.Tags.ToTagsString().Should().Be("age=35");
        });

        commandResults.Items.Count.Should().Be(1);
        Cursor<GraphQueryResult> resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.AddNode);
            x.Status.IsOk().Should().BeTrue();
            x.Items.Should().NotBeNull();
        });

        var lookupOption = _map.Execute("select (key=node6);", NullScopeContext.Instance);
        lookupOption.IsOk().Should().BeTrue(newMapOption.ToString());

        GraphQueryResults lookupResults = lookupOption.Return();
        lookupResults.Items.Count.Should().Be(1);
        lookupResults.Get<GraphNode>().First().Action(x =>
        {
            x.Key.Should().Be("node6");
            x.Tags.ToTagsString().Should().Be("age=35");
        });
    }

    [Fact]
    public void UpsertForEdge()
    {
        var copyMap = _map.Copy();
        var newMapOption = _map.Execute("upsert edge fromKey=node6, toKey=node3, edgeType=default, newTags;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(copyMap, _map);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Should().Be("node6");
            x.ToKey.Should().Be("node3");
            x.EdgeType.Should().Be("default");
            x.Tags.ToTagsString().Should().Be("created,newTags");
        });

        commandResults.Items.Count.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.AddEdge);
            x.Status.IsOk().Should().BeTrue();
            x.Items.Should().NotBeNull();
        });
    }

    [Fact]
    public void SingleAddWithUpsertForNode()
    {
        var copyMap = _map.Copy();
        var newMapOption = _map.Execute("upsert node key=node99, newTags;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(copyMap, _map);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node99");
            x.Tags.ToTagsString().Should().Be("newTags");
        });

        commandResults.Items.Count.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.AddNode);
            x.Status.IsOk().Should().BeTrue();
            x.Items.Should().NotBeNull();
        });
    }

    [Fact]
    public void SingleAddWithUpsertForNodeWithMultipleTags()
    {
        var copyMap = _map.Copy();
        var newMapOption = _map.Execute("upsert node key=node99, newTags,label=client;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(copyMap, _map);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphNode>().Action(x =>
        {
            x.Key.Should().Be("node99");
            x.Tags.ToTagsString().Should().Be("label=client,newTags");
        });

        commandResults.Items.Count.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.AddNode);
            x.Status.IsOk().Should().BeTrue();
            x.Items.Should().NotBeNull();
        });
    }

    [Fact]
    public void SingleAddWithUpsertForEdge()
    {
        var copyMap = _map.Copy();
        var newMapOption = _map.Execute("upsert edge fromKey=node7, toKey=node1, edgeType=newEdgeType, newTags;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue(newMapOption.ToString());

        GraphQueryResults commandResults = newMapOption.Return();
        var compareMap = GraphCommandTools.CompareMap(copyMap, _map);

        compareMap.Count.Should().Be(1);
        compareMap[0].Cast<GraphEdge>().Action(x =>
        {
            x.FromKey.Should().Be("node7");
            x.ToKey.Should().Be("node1");
            x.EdgeType.Should().Be("newEdgeType");
            x.Tags.ToTagsString().Should().Be("newTags");
        });

        commandResults.Items.Count.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.AddEdge);
            x.Status.IsOk().Should().BeTrue();
            x.Items.Should().NotBeNull();
        });
    }
}
