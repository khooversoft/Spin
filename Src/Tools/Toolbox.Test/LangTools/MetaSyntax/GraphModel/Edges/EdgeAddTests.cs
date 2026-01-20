using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax.GraphModel.Edges;

public class EdgeAddTests
{
    private readonly MetaSyntaxRoot _root;
    private readonly SyntaxParser _parser;

    public EdgeAddTests(ITestOutputHelper output)
    {
        var host = Host.CreateDefaultBuilder()
            .AddDebugLogging(x => output.WriteLine(x))
            .Build();

        string schema = GraphModelTool.ReadGraphLanauge2();
        _root = MetaParser.ParseRules(schema);
        _root.StatusCode.IsOk().BeTrue(_root.Error);

        _parser = ActivatorUtilities.CreateInstance<SyntaxParser>(host.Services, _root);
    }

    [Theory]
    [InlineData("add")]
    [InlineData("add edge")]
    [InlineData("add node key")]
    [InlineData("add node from=")]
    [InlineData("add node from=k1")]
    [InlineData("add node from=k1, to=k2")]
    [InlineData("add node from=k1, to=k2, ")]
    [InlineData("add node from=k1, to=k2, edge=e1, set data { base64 } ;")]
    [InlineData("add edge from=fkey1, to=tkey1, type=label set set t1;")]
    [InlineData("()")]
    [InlineData("();")]
    [InlineData("data { hexdata };")]
    public void FailedReturn(string command)
    {
        var parse = _parser.Parse(command);
        parse.Status.IsError().BeTrue(parse.Status.Error);
    }

    [Fact]
    public void MinAddCommand()
    {
        var parse = _parser.Parse("add edge from=fkey1, to=tkey1, type=label;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("add"), Name = "add-sym" },
            new SyntaxPair { Token = new TokenValue("edge"), Name = "edge-sym" },
            new SyntaxPair { Token = new TokenValue("from"), Name = "fromKey-sym" },
            new SyntaxPair { Token = new TokenValue("="), Name = "equal" },
            new SyntaxPair { Token = new TokenValue("fkey1"), Name = "fromKey-value" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("to"), Name = "toKey-sym" },
            new SyntaxPair { Token = new TokenValue("="), Name = "equal" },
            new SyntaxPair { Token = new TokenValue("tkey1"), Name = "toKey-value" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("type"), Name = "edgeType-sym" },
            new SyntaxPair { Token = new TokenValue("="), Name = "equal" },
            new SyntaxPair { Token = new TokenValue("label"), Name = "edgeType-value" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }

    [Fact]
    public void AddWithSetTagCommand()
    {
        var parse = _parser.Parse("add edge from=fkey1, to=tkey1, type=label set t1;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("add"), Name = "add-sym" },
            new SyntaxPair { Token = new TokenValue("edge"), Name = "edge-sym" },
            new SyntaxPair { Token = new TokenValue("from"), Name = "fromKey-sym" },
            new SyntaxPair { Token = new TokenValue("="), Name = "equal" },
            new SyntaxPair { Token = new TokenValue("fkey1"), Name = "fromKey-value" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("to"), Name = "toKey-sym" },
            new SyntaxPair { Token = new TokenValue("="), Name = "equal" },
            new SyntaxPair { Token = new TokenValue("tkey1"), Name = "toKey-value" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("type"), Name = "edgeType-sym" },
            new SyntaxPair { Token = new TokenValue("="), Name = "equal" },
            new SyntaxPair { Token = new TokenValue("label"), Name = "edgeType-value" },
            new SyntaxPair { Token = new TokenValue("set"), Name = "set-sym" },
            new SyntaxPair { Token = new TokenValue("t1"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }

    [Fact]
    public void AddWithSetMultipleTagCommand()
    {
        var parse = _parser.Parse("add edge from=fkey1, to=tkey1, type=label set t1, t2=v3, t3;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("add"), Name = "add-sym" },
            new SyntaxPair { Token = new TokenValue("edge"), Name = "edge-sym" },
            new SyntaxPair { Token = new TokenValue("from"), Name = "fromKey-sym" },
            new SyntaxPair { Token = new TokenValue("="), Name = "equal" },
            new SyntaxPair { Token = new TokenValue("fkey1"), Name = "fromKey-value" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("to"), Name = "toKey-sym" },
            new SyntaxPair { Token = new TokenValue("="), Name = "equal" },
            new SyntaxPair { Token = new TokenValue("tkey1"), Name = "toKey-value" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("type"), Name = "edgeType-sym" },
            new SyntaxPair { Token = new TokenValue("="), Name = "equal" },
            new SyntaxPair { Token = new TokenValue("label"), Name = "edgeType-value" },
            new SyntaxPair { Token = new TokenValue("set"), Name = "set-sym" },
            new SyntaxPair { Token = new TokenValue("t1"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("t2"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("v3"), Name = "tagValue" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("t3"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }
}
