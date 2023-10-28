using FluentAssertions;
using Toolbox.Data.Graph;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Test.Data;

public class GraphQueryParserTests
{
    [Fact]
    public void NoCommand()
    {
        Option<IReadOnlyList<QueryCmd>> option = GraphCmdParser.Parse(null!);
        option.IsError().Should().BeTrue();

        option = GraphCmdParser.Parse("");
        option.IsError().Should().BeTrue();

        option = GraphCmdParser.Parse("a");
        option.IsError().Should().BeTrue();

        option = GraphCmdParser.Parse("a=");
        option.IsError().Should().BeTrue();
    }

    [Fact]
    public void SingleValue()
    {
        Option<IReadOnlyList<QueryCmd>> option = GraphCmdParser.Parse("k=v");
        option.IsOk().Should().BeTrue();
        option.Return().Action(x =>
        {
            x.Count.Should().Be(1);
            x[0].Symbol.Should().Be("k");
            x[0].Opr.Should().Be(QueryOpr.Equal);
            x[0].Value.Should().Be("v");
        });

        option = GraphCmdParser.Parse("name='next value'");
        option.IsOk().Should().BeTrue();
        option.Return().Action(x =>
        {
            x.Count.Should().Be(1);
            x[0].Symbol.Should().Be("name");
            x[0].Opr.Should().Be(QueryOpr.Equal);
            x[0].Value.Should().Be("next value");
        });
    }

    [Fact]
    public void NultipleValuesWithAnd()
    {
        Option<IReadOnlyList<QueryCmd>> option = GraphCmdParser.Parse("k=v&&n=v1");
        option.IsOk().Should().BeTrue();

        var data = option.Return();
        data.Count.Should().Be(3);

        data[0].Symbol.Should().Be("k");
        data[0].Opr.Should().Be(QueryOpr.Equal);
        data[0].Value.Should().Be("v");

        data[1].Opr.Should().Be(QueryOpr.And);

        data[2].Symbol.Should().Be("n");
        data[2].Opr.Should().Be(QueryOpr.Equal);
        data[2].Value.Should().Be("v1");
    }

    [Fact]
    public void NultipleValuesWithSema()
    {
        var option = GraphCmdParser.Parse("k=v;n=v1;o1=v2");
        option.IsOk().Should().BeTrue();

        var data = option.Return();

        data.Count.Should().Be(5);
        int index = -1;

        data[++index].Symbol.Should().Be("k");
        data[index].Opr.Should().Be(QueryOpr.Equal);
        data[index].Value.Should().Be("v");

        data[++index].Opr.Should().Be(QueryOpr.Semicolon);

        data[++index].Symbol.Should().Be("n");
        data[index].Opr.Should().Be(QueryOpr.Equal);
        data[index].Value.Should().Be("v1");

        data[++index].Opr.Should().Be(QueryOpr.Semicolon);

        data[++index].Symbol.Should().Be("o1");
        data[index].Opr.Should().Be(QueryOpr.Equal);
        data[index].Value.Should().Be("v2");
    }

    [Fact]
    public void MultipleValueWithAndAndSpace()
    {
        var option = GraphCmdParser.Parse("k=v && n=v1 && o1=v2");
        option.IsOk().Should().BeTrue();

        var data = option.Return();

        data.Count.Should().Be(5);
        int index = -1;

        data[++index].Symbol.Should().Be("k");
        data[index].Opr.Should().Be(QueryOpr.Equal);
        data[index].Value.Should().Be("v");

        data[++index].Opr.Should().Be(QueryOpr.And);

        data[++index].Symbol.Should().Be("n");
        data[index].Opr.Should().Be(QueryOpr.Equal);
        data[index].Value.Should().Be("v1");

        data[++index].Opr.Should().Be(QueryOpr.And);

        data[++index].Symbol.Should().Be("o1");
        data[index].Opr.Should().Be(QueryOpr.Equal);
        data[index].Value.Should().Be("v2");
    }

    [Fact]
    public void Example()
    {
        var option = GraphCmdParser.Parse("toKey = 'value1' && fromKey = 'value 2' && tags has 't2=v2' && tokey match 'schema:*'");
        option.IsOk().Should().BeTrue();

        var data = option.Return();

        data.Count.Should().Be(7);
        int index = -1;

        data[++index].Symbol.Should().Be("toKey");
        data[index].Opr.Should().Be(QueryOpr.Equal);
        data[index].Value.Should().Be("value1");

        data[++index].Opr.Should().Be(QueryOpr.And);

        data[++index].Symbol.Should().Be("fromKey");
        data[index].Opr.Should().Be(QueryOpr.Equal);
        data[index].Value.Should().Be("value 2");

        data[++index].Opr.Should().Be(QueryOpr.And);

        data[++index].Symbol.Should().Be("tags");
        data[index].Opr.Should().Be(QueryOpr.Has);
        data[index].Value.Should().Be("t2=v2");

        data[++index].Opr.Should().Be(QueryOpr.And);

        data[++index].Symbol.Should().Be("tokey");
        data[index].Opr.Should().Be(QueryOpr.Match);
        data[index].Value.Should().Be("schema:*");
    }
}
