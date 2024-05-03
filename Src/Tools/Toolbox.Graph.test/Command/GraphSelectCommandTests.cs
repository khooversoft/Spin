﻿using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Command;

public class GraphSelectCommandTests
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
    public void LookupSingleNode()
    {
        var copyMap = _map.Copy();
        var newMapOption = _map.Execute("select (key=node4);", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        GraphQueryResults commandResults = newMapOption.Return();
        GraphCommandTools.CompareMap(copyMap, _map).Count.Should().Be(0);

        commandResults.Items.Length.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.Select);
            x.Status.IsOk().Should().BeTrue();
            x.ReturnNames.Count.Should().Be(0);

            x.Items.NotNull().Length.Should().Be(1);
            var index = x.Items.NotNull().ToCursor();

            index.NextValue().Return().Cast<GraphNode>().Action(x =>
            {
                x.Key.Should().Be("node4");
                x.Tags.ToTagsString().Should().Be("age=32,name=josh");
                x.Links.Count.Should().Be(0);
                x.DataMap.Count.Should().Be(0);
            });
        });
    }

    [Fact]
    public void LookupSingleNodeWithReturn()
    {
        var copyMap = _map.Copy();
        var newMapOption = _map.Execute("select (key=node4) return lease;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        GraphQueryResults commandResults = newMapOption.Return();
        GraphCommandTools.CompareMap(copyMap, _map).Count.Should().Be(0);

        commandResults.Items.Length.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.Select);
            x.Status.IsOk().Should().BeTrue();
            x.ReturnNames.Count.Should().Be(1);
            Enumerable.SequenceEqual(x.ReturnNames.OrderBy(x => x), ["lease"]).Should().BeTrue();

            x.Items.NotNull().Length.Should().Be(1);
            var index = x.Items.NotNull().ToCursor();

            index.NextValue().Return().Cast<GraphNode>().Action(x =>
            {
                x.Key.Should().Be("node4");
                x.Tags.ToTagsString().Should().Be("age=32,name=josh");
                x.Links.Count.Should().Be(0);
                x.DataMap.Count.Should().Be(0);
            });
        });
    }

    [Fact]
    public void LookupSingleNodeWithTwoReturn()
    {
        var copyMap = _map.Copy();
        var newMapOption = _map.Execute("select (key=node4) return lease, contract;", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        GraphQueryResults commandResults = newMapOption.Return();
        GraphCommandTools.CompareMap(copyMap, _map).Count.Should().Be(0);

        commandResults.Items.Length.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.Select);
            x.Status.IsOk().Should().BeTrue();
            x.ReturnNames.Count.Should().Be(2);
            Enumerable.SequenceEqual(x.ReturnNames.OrderBy(x => x), ["contract", "lease"]).Should().BeTrue();

            x.Items.NotNull().Length.Should().Be(1);
            var index = x.Items.NotNull().ToCursor();

            index.NextValue().Return().Cast<GraphNode>().Action(x =>
            {
                x.Key.Should().Be("node4");
                x.Tags.ToTagsString().Should().Be("age=32,name=josh");
                x.Links.Count.Should().Be(0);
                x.DataMap.Count.Should().Be(0);
            });
        });
    }

    [Fact]
    public void SingleSelectForNode()
    {
        var copyMap = _map.Copy();
        var newMapOption = _map.Execute("select (lang=java);", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        GraphQueryResults commandResults = newMapOption.Return();
        GraphCommandTools.CompareMap(copyMap, _map).Count.Should().Be(0);

        commandResults.Items.Length.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.Select);
            x.Status.IsOk().Should().BeTrue();

            x.Items.NotNull().Length.Should().Be(3);
            var index = x.Items.NotNull().ToCursor();

            index.NextValue().Return().Cast<GraphNode>().Action(x =>
            {
                x.Key.Should().Be("node3");
                x.Tags.ToTagsString().Should().Be("lang=java,name=lop");
            });

            index.NextValue().Return().Cast<GraphNode>().Action(x =>
            {
                x.Key.Should().Be("node5");
                x.Tags.ToTagsString().Should().Be("lang=java,name=ripple");
            });

            index.NextValue().Return().Cast<GraphNode>().Action(x =>
            {
                x.Key.Should().Be("node7");
                x.Tags.ToTagsString().Should().Be("lang=java");
            });
        });
    }

    [Fact]
    public void SingleSelectForEdge()
    {
        var copyMap = _map.Copy();
        var newMapOption = _map.Execute("select [knows];", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        GraphQueryResults commandResults = newMapOption.Return();
        GraphCommandTools.CompareMap(copyMap, _map).Count.Should().Be(0);

        commandResults.Items.Length.Should().Be(1);
        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.Select);
            x.Status.IsOk().Should().BeTrue();

            x.Items.NotNull().Length.Should().Be(2);

            var index = x.Items.NotNull().ToCursor();
            index.NextValue().Return().Cast<GraphEdge>().Action(x =>
            {
                x.FromKey.Should().Be("node1");
                x.ToKey.Should().Be("node2");
                x.Tags.ToTagsString().Should().Be("knows,level=1");
            });

            index.NextValue().Return().Cast<GraphEdge>().Action(x =>
            {
                x.FromKey.Should().Be("node1");
                x.ToKey.Should().Be("node3");
                x.Tags.ToTagsString().Should().Be("knows,level=1");
            });
        });
    }


    [Fact]
    public void TwoSelectCommandQuery()
    {
        var copyMap = _map.Copy();
        var newMapOption = _map.Execute("select [knows];select [created];", NullScopeContext.Instance);
        newMapOption.IsOk().Should().BeTrue();

        GraphQueryResults commandResults = newMapOption.Return();
        GraphCommandTools.CompareMap(copyMap, _map).Count.Should().Be(0);
        commandResults.Items.Length.Should().Be(2);

        var resultIndex = commandResults.Items.ToCursor();

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.Select);
            x.Status.IsOk().Should().BeTrue();

            x.Items.NotNull().Length.Should().Be(2);
            var index = x.Items.NotNull().ToCursor();

            index.NextValue().Return().Cast<GraphEdge>().Action(x =>
            {
                x.FromKey.Should().Be("node1");
                x.ToKey.Should().Be("node2");
                x.Tags.ToTagsString().Should().Be("knows,level=1");
            });

            index.NextValue().Return().Cast<GraphEdge>().Action(x =>
            {
                x.FromKey.Should().Be("node1");
                x.ToKey.Should().Be("node3");
                x.Tags.ToTagsString().Should().Be("knows,level=1");
            });
        });

        resultIndex.NextValue().Return().Action(x =>
        {
            x.CommandType.Should().Be(CommandType.Select);
            x.Status.IsOk().Should().BeTrue();

            x.Items.NotNull().Length.Should().Be(3);
            var index = x.Items.NotNull().ToCursor();

            index.NextValue().Return().Cast<GraphEdge>().Action(x =>
            {
                x.FromKey.Should().Be("node6");
                x.ToKey.Should().Be("node3");
                x.Tags.ToTagsString().Should().Be("created");
            });

            index.NextValue().Return().Cast<GraphEdge>().Action(x =>
            {
                x.FromKey.Should().Be("node4");
                x.ToKey.Should().Be("node5");
                x.Tags.ToTagsString().Should().Be("created");
            });

            index.NextValue().Return().Cast<GraphEdge>().Action(x =>
            {
                x.FromKey.Should().Be("node4");
                x.ToKey.Should().Be("node3");
                x.Tags.ToTagsString().Should().Be("created");
            });
        });
    }
}
