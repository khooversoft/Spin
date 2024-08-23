using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Test.Application;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax.GraphModel.Relationships;

public class DeleteRelationshipTests : TestBase<SelectRelationshipTests>
{
    private readonly ITestOutputHelper _output;
    private readonly MetaSyntaxRoot _root;
    private readonly SyntaxParser _parser;
    private readonly ScopeContext _context;

    public DeleteRelationshipTests(ITestOutputHelper output) : base(output)
    {
        _output = output.NotNull();

        string schema = GraphModelTool.ReadGraphLanauge2();
        _root = MetaParser.ParseRules(schema);
        _root.StatusCode.IsOk().Should().BeTrue(_root.Error);

        _context = GetScopeContext();
        _parser = new SyntaxParser(_root);
    }

    [Fact]
    public void DeleteNodesRelationship()
    {
        var parse = _parser.Parse("delete (*) ;", _context);
        parse.StatusCode.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("delete"), MetaSyntaxName = "delete-sym" },
            new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void DeleteAllRelationshipsAndReturnDataCommand()
    {
        var parse = _parser.Parse("delete [*] ;", _context);
        parse.StatusCode.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("delete"), MetaSyntaxName = "delete-sym" },
            new SyntaxPair { Token = new TokenValue("["), MetaSyntaxName = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue("]"), MetaSyntaxName = "close-bracket" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void DeleteNodesToEdgeRelationship()
    {
        var parse = _parser.Parse("delete (*) -> [*] ;", _context);
        parse.StatusCode.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair {Token = new TokenValue("delete"), MetaSyntaxName = "delete-sym"},
            new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
            new SyntaxPair { Token = new TokenValue("->"), MetaSyntaxName = "left-join" },
            new SyntaxPair { Token = new TokenValue("["), MetaSyntaxName = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue("]"), MetaSyntaxName = "close-bracket" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void DeleteNodesWithAliasToEdgeRelationship()
    {
        var parse = _parser.Parse("delete (*) a1 -> [*] ;", _context);
        parse.StatusCode.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair {Token = new TokenValue("delete"), MetaSyntaxName = "delete-sym"},
            new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
            new SyntaxPair { Token = new TokenValue("a1"), MetaSyntaxName = "alias" },
            new SyntaxPair { Token = new TokenValue("->"), MetaSyntaxName = "left-join" },
            new SyntaxPair { Token = new TokenValue("["), MetaSyntaxName = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue("]"), MetaSyntaxName = "close-bracket" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void DeleteNodesToEdgeToNodeRelationship()
    {
        var parse = _parser.Parse("delete (*) -> [*] -> (*) ;", _context);
        parse.StatusCode.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair {Token = new TokenValue("delete"), MetaSyntaxName = "delete-sym"},
            new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
            new SyntaxPair { Token = new TokenValue("->"), MetaSyntaxName = "left-join" },
            new SyntaxPair { Token = new TokenValue("["), MetaSyntaxName = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue("]"), MetaSyntaxName = "close-bracket" },
            new SyntaxPair { Token = new TokenValue("->"), MetaSyntaxName = "left-join" },
            new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void DeleteNodesToEdgeToNodeWithAliasRelationship()
    {
        var parse = _parser.Parse("delete (*) a1 -> [*] a2 -> (*) a3 ;", _context);
        parse.StatusCode.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("delete"), MetaSyntaxName = "delete-sym" },
            new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
            new SyntaxPair { Token = new TokenValue("a1"), MetaSyntaxName = "alias" },
            new SyntaxPair { Token = new TokenValue("->"), MetaSyntaxName = "left-join" },
            new SyntaxPair { Token = new TokenValue("["), MetaSyntaxName = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue("]"), MetaSyntaxName = "close-bracket" },
            new SyntaxPair { Token = new TokenValue("a2"), MetaSyntaxName = "alias" },
            new SyntaxPair { Token = new TokenValue("->"), MetaSyntaxName = "left-join" },
            new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
            new SyntaxPair { Token = new TokenValue("a3"), MetaSyntaxName = "alias" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void DeleteNotCorrectButWorksRelationship()
    {
        var parse = _parser.Parse("delete  (*) -> (*) -> (*) ;", _context);
        parse.StatusCode.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair {Token = new TokenValue("delete"), MetaSyntaxName = "delete-sym"},
            new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
            new SyntaxPair { Token = new TokenValue("->"), MetaSyntaxName = "left-join" },
            new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
            new SyntaxPair { Token = new TokenValue("->"), MetaSyntaxName = "left-join" },
            new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
            new SyntaxPair { Token = new TokenValue(";"), MetaSyntaxName = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }
}