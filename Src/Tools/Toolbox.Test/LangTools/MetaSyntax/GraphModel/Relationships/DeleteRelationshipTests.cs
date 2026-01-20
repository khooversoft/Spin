using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax.GraphModel.Relationships;

public class DeleteRelationshipTests
{
    private readonly MetaSyntaxRoot _root;
    private readonly SyntaxParser _parser;

    public DeleteRelationshipTests(ITestOutputHelper output)
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
    public void DeleteNodesRelationship()
    {
        var parse = _parser.Parse("delete (*) ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("delete"), Name = "delete-sym" },
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }

    [Fact]
    public void DeleteAllRelationshipsAndReturnDataCommand()
    {
        var parse = _parser.Parse("delete [*] ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("delete"), Name = "delete-sym" },
            new SyntaxPair { Token = new TokenValue("["), Name = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue("]"), Name = "close-bracket" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }

    [Fact]
    public void DeleteNodesToEdgeRelationship()
    {
        var parse = _parser.Parse("delete (*) -> [*] ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("delete"), Name = "delete-sym" },
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
            new SyntaxPair { Token = new TokenValue("->"), Name = "left-join" },
            new SyntaxPair { Token = new TokenValue("["), Name = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue("]"), Name = "close-bracket" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }

    [Fact]
    public void DeleteNodesWithAliasToEdgeRelationship()
    {
        var parse = _parser.Parse("delete (*) a1 -> [*] ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("delete"), Name = "delete-sym" },
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
            new SyntaxPair { Token = new TokenValue("a1"), Name = "alias" },
            new SyntaxPair { Token = new TokenValue("->"), Name = "left-join" },
            new SyntaxPair { Token = new TokenValue("["), Name = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue("]"), Name = "close-bracket" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }

    [Fact]
    public void DeleteNodesToEdgeToNodeRelationship()
    {
        var parse = _parser.Parse("delete (*) -> [*] -> (*) ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("delete"), Name = "delete-sym" },
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
            new SyntaxPair { Token = new TokenValue("->"), Name = "left-join" },
            new SyntaxPair { Token = new TokenValue("["), Name = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue("]"), Name = "close-bracket" },
            new SyntaxPair { Token = new TokenValue("->"), Name = "left-join" },
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }

    [Fact]
    public void DeleteNodesToEdgeToNodeWithAliasRelationship()
    {
        var parse = _parser.Parse("delete (*) a1 -> [*] a2 -> (*) a3 ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("delete"), Name = "delete-sym" },
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
            new SyntaxPair { Token = new TokenValue("a1"), Name = "alias" },
            new SyntaxPair { Token = new TokenValue("->"), Name = "left-join" },
            new SyntaxPair { Token = new TokenValue("["), Name = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue("]"), Name = "close-bracket" },
            new SyntaxPair { Token = new TokenValue("a2"), Name = "alias" },
            new SyntaxPair { Token = new TokenValue("->"), Name = "left-join" },
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
            new SyntaxPair { Token = new TokenValue("a3"), Name = "alias" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }

    [Fact]
    public void DeleteNotCorrectButWorksRelationship()
    {
        var parse = _parser.Parse("delete  (*) -> (*) -> (*) ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("delete"), Name = "delete-sym" },
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
            new SyntaxPair { Token = new TokenValue("->"), Name = "left-join" },
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
            new SyntaxPair { Token = new TokenValue("->"), Name = "left-join" },
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "tagKey" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }
}