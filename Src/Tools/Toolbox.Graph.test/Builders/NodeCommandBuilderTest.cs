using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.test.Builders;

public class NodeCommandBuilderTest
{
    [Fact]
    public void AddNodeOnly()
    {
        var graphQuery = new NodeCommandBuilder()
            .SetNodeKey("nodeKey1")
            .Build();

        graphQuery.Be("add node key=nodeKey1 ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Instance);
        parse.Status.IsOk().BeTrue();
    }

    [Fact]
    public void SetNodeOnly()
    {
        var graphQuery = new NodeCommandBuilder()
            .UseSet()
            .SetNodeKey("nodeKey1")
            .Build();

        graphQuery.Be("set node key=nodeKey1 ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Instance);
        parse.Status.IsOk().BeTrue();
    }

    [Fact]
    public void Tags()
    {
        var graphQuery = new NodeCommandBuilder()
            .SetNodeKey("nodeKey1")
            .AddTag("tag", "value")
            .Build();

        graphQuery.Be("add node key=nodeKey1 set tag=value ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Instance);
        parse.Status.IsOk().BeTrue();
    }

    [Fact]
    public void TagWithNoValue()
    {
        var graphQuery = new NodeCommandBuilder()
            .SetNodeKey("nodeKey1")
            .AddTag("t1")
            .Build();

        graphQuery.Be("add node key=nodeKey1 set t1 ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Instance);
        parse.Status.IsOk().BeTrue();
    }

    [Fact]
    public void TagFormat()
    {
        var graphQuery = new NodeCommandBuilder()
            .SetNodeKey("nodeKey1")
            .AddTag("t1=v1")
            .Build();

        graphQuery.Be("add node key=nodeKey1 set t1=v1 ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Instance);
        parse.Status.IsOk().BeTrue();
    }

    [Fact]
    public void TagFormat2()
    {
        var graphQuery = new NodeCommandBuilder()
            .SetNodeKey("nodeKey1")
            .AddTag("t1=v1")
            .AddTag("t2=v2")
            .Build();

        graphQuery.Be("add node key=nodeKey1 set t1=v1,t2=v2 ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Instance);
        parse.Status.IsOk().BeTrue();
    }

    [Fact]
    public void TagRemove()
    {
        var graphQuery = new NodeCommandBuilder()
            .UseSet()
            .SetNodeKey("nodeKey1")
            .AddTag("-t1")
            .Build();

        graphQuery.Be("set node key=nodeKey1 set -t1 ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Instance);
        parse.Status.IsOk().BeTrue();
    }

    [Fact]
    public void TwoTags()
    {
        var graphQuery = new NodeCommandBuilder()
            .SetNodeKey("nodeKey1")
            .AddTag("tag1", "value1")
            .AddTag("tag2", "value2")
            .Build();

        graphQuery.Be("add node key=nodeKey1 set tag1=value1,tag2=value2 ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Instance);
        parse.Status.IsOk().BeTrue();
    }

    [Fact]
    public void ForeignKeyNotTag()
    {
        var graphQuery = new NodeCommandBuilder()
            .SetNodeKey("nodeKey1")
            .AddForeignKey("t1")
            .Build();

        graphQuery.Be("add node key=nodeKey1 foreignkey t1 ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Instance);
        parse.Status.IsOk().BeTrue();
    }

    [Fact]
    public void Data()
    {
        var graphQuery = new NodeCommandBuilder()
            .SetNodeKey("nodeKey1")
            .AddData("data", "value")
            .Build();

        graphQuery.Be("add node key=nodeKey1 set data { 'value' } ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Instance);
        parse.Status.IsOk().BeTrue();
    }

    [Fact]
    public void TwoData()
    {
        var graphQuery = new NodeCommandBuilder()
            .SetNodeKey("nodeKey1")
            .AddData("data1", "value1")
            .AddData("data2", "value2")
            .Build();

        graphQuery.Be("add node key=nodeKey1 set data1 { 'value1' }, data2 { 'value2' } ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Instance);
        parse.Status.IsOk().BeTrue();
    }

    [Fact]
    public void Indexes()
    {
        var graphQuery = new NodeCommandBuilder()
            .SetNodeKey("nodeKey1")
            .AddIndex("t1")
            .Build();

        graphQuery.Be("add node key=nodeKey1 index t1 ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Instance);
        parse.Status.IsOk().BeTrue();
    }

    [Fact]
    public void TwoIndexes()
    {
        var graphQuery = new NodeCommandBuilder()
            .SetNodeKey("nodeKey1")
            .AddIndex("t1")
            .AddIndex("t2")
            .Build();

        graphQuery.Be("add node key=nodeKey1 index t1, t2 ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Instance);
        parse.Status.IsOk().BeTrue();
    }

    [Fact]
    public void FullParameters()
    {
        var graphQuery = new NodeCommandBuilder()
            .SetNodeKey("nodeKey1")
            .AddTag("tag", "value")
            .AddData("data", "value")
            .AddIndex("t1")
            .Build();

        graphQuery.Be("add node key=nodeKey1 set tag=value, data { 'value' } index t1 ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Instance);
        parse.Status.IsOk().BeTrue();
    }

    [Fact]
    public void ForeignKey()
    {
        var graphQuery = new NodeCommandBuilder()
            .SetNodeKey("nodeKey1")
            .AddTag("t1", "value1")
            .AddTag("t2", "value2")
            .AddForeignKey("t1")
            .AddForeignKey("t2")
            .Build();

        graphQuery.Be("add node key=nodeKey1 set t1=value1,t2=value2 foreignkey t1,t2 ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Instance);
        parse.Status.IsOk().BeTrue();
    }

    [Fact]
    public void ForignKeyWithPattern()
    {
        var graphQuery = new NodeCommandBuilder()
            .SetNodeKey("nodeKey1")
            .AddTag("t1", "value1")
            .AddTag("t2", "value2")
            .AddForeignKey("t1")
            .AddForeignKey("t2=pattern*")
            .Build();

        graphQuery.Be("add node key=nodeKey1 set t1=value1,t2=value2 foreignkey t1,t2=pattern* ;");

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Instance);
        parse.Status.IsOk().BeTrue();
    }


    [Fact]
    public void Reference()
    {
        var graphQuery = new NodeCommandBuilder()
            .SetNodeKey("nodeKey1")
            .AddIndex("t1")
            .AddReferences("edgeType", ["path1", "path2"])
            .Build();

        var expected = "add node key=nodeKey1 set edgeType-*=path1,edgeType-*=path2 index t1 foreignkey edgeType=edgeType-* ;";
        graphQuery.Match(expected);

        var parse = GraphLanguageTool.GetSyntaxRoot().Parse(graphQuery, NullScopeContext.Instance);
        parse.Status.IsOk().BeTrue();
    }
}
