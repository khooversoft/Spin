using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class OptionalWithPrefix
{
    private readonly MetaSyntaxRoot _schema;
    private readonly SyntaxParser _parser;

    public OptionalWithPrefix(ITestOutputHelper output)
    {
        string schemaText = new[]
        {
            "delimiters          = '->' '<->' ( ) [ ] ;",
            "symbol              = regex '^[a-zA-Z\\*][a-zA-Z0-9\\-\\*]*$' ;",
            "join-left           = '->' ;",
            "join-inner          = '<->' ;",
            "join                = ( join-left | join-inner ) ;",
            "select              = symbol, [ join ] ;",
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().BeTrue();

        var host = Host.CreateDefaultBuilder()
            .AddDebugLogging(x => output.WriteLine(x))
            .Build();

        _parser = ActivatorUtilities.CreateInstance<SyntaxParser>(host.Services, _schema);
    }

    [Fact]
    public void NoJoin()
    {
        var parse = _parser.Parse("first");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "select",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("first"), Name = "symbol" },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("first"), Name = "symbol" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void LeftJoin()
    {
        var parse = _parser.Parse("first ->");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "select",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("first"), Name = "symbol" },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_select-3-OptionGroup",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxTree
                                {
                                    MetaSyntaxName = "join",
                                    Children = new ISyntaxTree[]
                                    {
                                        new SyntaxTree
                                        {
                                            MetaSyntaxName = "_join-1-OrGroup",
                                            Children = new ISyntaxTree[]
                                            {
                                                new SyntaxPair { Token = new TokenValue("->"), Name = "join-left" },
                                            },
                                        },
                                    },
                                },
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
            new SyntaxPair { Token = new TokenValue("first"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("->"), Name = "join-left" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void InnerJoin()
    {
        var parse = _parser.Parse("first <->");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "select",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("first"), Name = "symbol" },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_select-3-OptionGroup",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxTree
                                {
                                    MetaSyntaxName = "join",
                                    Children = new ISyntaxTree[]
                                    {
                                        new SyntaxTree
                                        {
                                            MetaSyntaxName = "_join-1-OrGroup",
                                            Children = new ISyntaxTree[]
                                            {
                                                new SyntaxPair { Token = new TokenValue("<->"), Name = "join-inner" },
                                            },
                                        },
                                    },
                                },
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
            new SyntaxPair { Token = new TokenValue("first"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("<->"), Name = "join-inner" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }
}
