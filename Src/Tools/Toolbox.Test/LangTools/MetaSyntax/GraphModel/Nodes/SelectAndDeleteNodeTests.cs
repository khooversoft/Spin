using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Logging;
using Toolbox.Test.Application;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax.GraphModel.Nodes;

public class SelectAndDeleteNodeTests : TestBase
{
    private readonly ITestOutputHelper _output;
    private readonly MetaSyntaxRoot _root;
    private readonly SyntaxParser _parser;
    private readonly ScopeContext _context;

    public SelectAndDeleteNodeTests(ITestOutputHelper output) : base(output)
    {
        _output = output.NotNull();

        string schema = GraphModelTool.ReadGraphLanauge2();
        _root = MetaParser.ParseRules(schema);
        _root.StatusCode.IsOk().Should().BeTrue(_root.Error);

        var services = new ServiceCollection()
            .AddLogging(x =>
            {
                x.AddLambda(_output.WriteLine);
                x.AddDebug();
                x.AddConsole();
            })
            .BuildServiceProvider();

        var logger = services.GetService<ILogger<NodeAddTests>>().NotNull();
        _context = new ScopeContext(logger);

        _parser = new SyntaxParser(_root);
    }

    [Fact]
    public void SelectAllNodesCommand()
    {
        var parse = _parser.Parse("select (*) ;", _context);
        parse.StatusCode.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("select"), MetaSyntaxName = "select-sym" },
            new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void SelectAllNodesAndReturnDataCommand()
    {
        var parse = _parser.Parse("select (*) return data, entity ;", _context);
        parse.StatusCode.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("select"), MetaSyntaxName = "select-sym" },
            new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
            new SyntaxPair { Token = new TokenValue("return"), MetaSyntaxName = "return-sym" },
            new SyntaxPair { Token = new TokenValue("data"), MetaSyntaxName = "dataName" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("entity"), MetaSyntaxName = "dataName" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void SelectNodeAndReturnDataCommand()
    {
        var parse = _parser.Parse("select (key=k1, t2) a1 return data, entity ;", _context);
        parse.StatusCode.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("select"), MetaSyntaxName = "select-sym" },
            new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
            new SyntaxPair { Token = new TokenValue("key"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("k1"), MetaSyntaxName = "tagValue" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("t2"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
            new SyntaxPair { Token = new TokenValue("a1"), MetaSyntaxName = "alias" },
            new SyntaxPair { Token = new TokenValue("return"), MetaSyntaxName = "return-sym" },
            new SyntaxPair { Token = new TokenValue("data"), MetaSyntaxName = "dataName" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("entity"), MetaSyntaxName = "dataName" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void SelectNodeByTypeCommand()
    {
        var parse = _parser.Parse("select (label) a1 ;", _context);
        parse.StatusCode.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("select"), MetaSyntaxName = "select-sym" },
            new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
            new SyntaxPair { Token = new TokenValue("label"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
            new SyntaxPair { Token = new TokenValue("a1"), MetaSyntaxName = "alias" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void DeleteNodeByTypeCommand()
    {
        var parse = _parser.Parse("delete (key=k1) a1 ;", _context);
        parse.StatusCode.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("delete"), MetaSyntaxName = "delete-sym" },
            new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
            new SyntaxPair { Token = new TokenValue("key"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("k1"), MetaSyntaxName = "tagValue" },
            new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
            new SyntaxPair { Token = new TokenValue("a1"), MetaSyntaxName = "alias" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }

}
