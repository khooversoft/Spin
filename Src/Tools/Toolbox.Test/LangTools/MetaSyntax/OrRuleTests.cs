using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class OrRuleTests
{
    private readonly MetaSyntaxRoot _schema;
    private readonly SyntaxParser _parser;

    public OrRuleTests(ITestOutputHelper output)
    {
        string schemaText = new[]
        {
            "delimiters = ( ) ;",
            "node-sym = 'node' ;",
            "edge-sym = 'edge' ;",
            "addCommand = ( node-sym | edge-sym ) ;"
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().BeTrue();

        var host = Host.CreateDefaultBuilder()
            .AddDebugLogging(x => output.WriteLine(x))
            .Build();

        _parser = ActivatorUtilities.CreateInstance<SyntaxParser>(host.Services, _schema);
    }

    [Fact]
    public void SimpleOrSymbol()
    {
        var parse = _parser.Parse("node");
        parse.Status.IsOk().BeTrue();

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "addCommand",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_addCommand-1-OrGroup",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair { Token = new TokenValue("node"), Name = "node-sym" },
                            },
                        },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("node"), Name = "node-sym" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void SimpleOrSymbolSecond()
    {
        var parse = _parser.Parse("edge");
        parse.Status.IsOk().BeTrue();

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "addCommand",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_addCommand-1-OrGroup",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair { Token = new TokenValue("edge"), Name = "edge-sym" },
                            },
                        },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("edge"), Name = "edge-sym" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void SimpleOrSymbolFail()
    {
        var parse = _parser.Parse("edxxxge");
        parse.Status.IsError().BeTrue();
    }
}
