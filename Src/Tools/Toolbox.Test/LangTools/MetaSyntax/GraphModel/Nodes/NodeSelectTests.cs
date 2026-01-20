using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax.GraphModel.Nodes;

public class NodeSelectTests
{
    private readonly MetaSyntaxRoot _root;
    private readonly SyntaxParser _parser;

    public NodeSelectTests(ITestOutputHelper output)
    {
        var host = Host.CreateDefaultBuilder()
            .AddDebugLogging(x => output.WriteLine(x))
            .Build();

        string schema = GraphModelTool.ReadGraphLanauge2();
        _root = MetaParser.ParseRules(schema);
        _root.StatusCode.IsOk().BeTrue(_root.Error);

        _parser = ActivatorUtilities.CreateInstance<SyntaxParser>(host.Services, _root);
    }

    [Fact]
    public void SelectAllNodesCommand()
    {
        var parse = _parser.Parse("select (*) ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("select"), Name = "select-sym" },
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }

    [Fact]
    public void SelectAllNodesAndReturnDataCommand()
    {
        var parse = _parser.Parse("select (*) return data, entity ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("select"), Name = "select-sym" },
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
            new SyntaxPair { Token = new TokenValue("return"), Name = "return-sym" },
            new SyntaxPair { Token = new TokenValue("data"), Name = "dataName" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("entity"), Name = "dataName" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }

    [Fact]
    public void SelectNodeAndReturnDataCommand()
    {
        var parse = _parser.Parse("select (key=k1, t2) a1 return data, entity ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("select"), Name = "select-sym" },
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("key"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("k1"), Name = "tagValue" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("t2"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
            new SyntaxPair { Token = new TokenValue("a1"), Name = "alias" },
            new SyntaxPair { Token = new TokenValue("return"), Name = "return-sym" },
            new SyntaxPair { Token = new TokenValue("data"), Name = "dataName" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("entity"), Name = "dataName" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }

    [Fact]
    public void SelectNodeByTypeCommand()
    {
        var parse = _parser.Parse("select (label) a1 ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("select"), Name = "select-sym" },
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("label"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
            new SyntaxPair { Token = new TokenValue("a1"), Name = "alias" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }
}
