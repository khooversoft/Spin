using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Test.Application;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class OptionalWithSuffix : TestBase
{
    private readonly MetaSyntaxRoot _schema;

    public OptionalWithSuffix(ITestOutputHelper output) : base(output)
    {
        string schemaText = new[]
        {
            "delimiters          = -> <-> [ ] ( ) ;",
            "symbol              = regex '^[a-zA-Z\\*][a-zA-Z0-9\\-\\*]*$' ;",
            "join-left           = '->' ;",
            "join-inner          = '<->' ;",
            "join                = [ (join-left | join-inner) ], symbol ;",
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().Should().BeTrue();
    }

    [Fact]
    public void NoJoin()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OptionalRuleOnly>();

        var parse = parser.Parse("first", logger);
        parse.Status.IsOk().Should().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "join",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("first"), MetaSyntaxName = "symbol" },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("first"), MetaSyntaxName = "symbol" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void LeftJoin()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OptionalRuleOnly>();

        var parse = parser.Parse("-> first", logger);
        parse.Status.IsOk().Should().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "join",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_join-1-OptionGroup",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxTree
                                {
                                    MetaSyntaxName = "_join-1-OptionGroup-1-OrGroup",
                                    Children = new ISyntaxTree[]
                                    {
                                        new SyntaxPair { Token = new TokenValue("->"), MetaSyntaxName = "join-left" },
                                    },
                                },
                            },
                        },
                        new SyntaxPair { Token = new TokenValue("first"), MetaSyntaxName = "symbol" },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("->"), MetaSyntaxName = "join-left" },
            new SyntaxPair { Token = new TokenValue("first"), MetaSyntaxName = "symbol" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void InnerJoin()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OptionalRuleOnly>();

        var parse = parser.Parse("<-> first", logger);
        parse.Status.IsOk().Should().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "join",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_join-1-OptionGroup",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxTree
                                {
                                    MetaSyntaxName = "_join-1-OptionGroup-1-OrGroup",
                                    Children = new ISyntaxTree[]
                                    {
                                        new SyntaxPair { Token = new TokenValue("<->"), MetaSyntaxName = "join-inner" },
                                    },
                                },
                            },
                        },
                        new SyntaxPair { Token = new TokenValue("first"), MetaSyntaxName = "symbol" },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("<->"), MetaSyntaxName = "join-inner" },
            new SyntaxPair { Token = new TokenValue("first"), MetaSyntaxName = "symbol" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).Should().BeTrue();
    }
}
