using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Test.Application;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax.GraphModel.Nodes;

public class SelectNodeQueryTests : TestBase
{
    private readonly MetaSyntaxRoot _schema;

    public SelectNodeQueryTests(ITestOutputHelper output) : base(output)
    {
        string schemaText = new[]
        {
            "delimiters          = , [ ] = ( ) -> <-> { } ;",
            "symbol              = regex '^[a-zA-Z\\*][a-zA-Z0-9\\-\\*]*$' ;",
            "alias               = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "tagValue            = string ;",
            "comma               = ',' ;",
            "tag                 = symbol, [ '=', tagValue ] ;",
            "tags                = tag, { comma, tag } ;",
            "open-bracket        = '[' #group-start #edge ;",
            "close-bracket       = ']' #group-end #edge ;",
            "open-param          = '(' #group-start #node ;",
            "close-param         = ')' #group-end #node ;",
            "join-left           = '->' ;",
            "join-inner          = '<->' ;",
            "node-spec           = open-param, tags, close-param, [ alias ] ;",
            "edge-spec           = open-bracket, tags, close-bracket, [ alias ] ;",
            "join                = ( join-left | join-inner ) ;",
            "node-edge-query     = ( node-spec | edge-spec ) ;",
            "select-node-query   = node-spec, { join, node-edge-query } ;",
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().Should().BeTrue(_schema.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("[]")]
    [InlineData("[*])")]
    [InlineData("[!*]")]
    public void FailedReturn(string command)
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse(command, logger);
        parse.Status.IsError().Should().BeTrue(parse.Status.Error);
    }


    [Fact]
    public void SelectNode()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("(*)", logger);
        parse.Status.IsOk().Should().BeTrue(parse.Status.Error);

        var lines = parse.SyntaxTree.GenerateTestCodeSyntaxTree().Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "select-node-query",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxTree
                        {
                            MetaSyntaxName = "node-spec",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
                                new SyntaxTree
                                {
                                    MetaSyntaxName = "tags",
                                    Children = new ISyntaxTree[]
                                    {
                                        new SyntaxTree
                                        {
                                            MetaSyntaxName = "tag",
                                            Children = new ISyntaxTree[]
                                            {
                                                new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "symbol" },
                                            },
                                        },
                                    },
                                },
                                new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
                            },
                        },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "symbol" },
            new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void SelectNodeToEdge()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("(*) -> [*]", logger);
        parse.Status.IsOk().Should().BeTrue(parse.Status.Error);

        var lines = parse.SyntaxTree.GenerateTestCodeSyntaxTree().Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "select-node-query",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxTree
                        {
                            MetaSyntaxName = "node-spec",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
                                new SyntaxTree
                                {
                                    MetaSyntaxName = "tags",
                                    Children = new ISyntaxTree[]
                                    {
                                        new SyntaxTree
                                        {
                                            MetaSyntaxName = "tag",
                                            Children = new ISyntaxTree[]
                                            {
                                                new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "symbol" },
                                            },
                                        },
                                    },
                                },
                                new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
                            },
                        },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_select-node-query-3-RepeatGroup",
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
                                                new SyntaxPair { Token = new TokenValue("->"), MetaSyntaxName = "join-left" },
                                            },
                                        },
                                    },
                                },
                                new SyntaxTree
                                {
                                    MetaSyntaxName = "node-edge-query",
                                    Children = new ISyntaxTree[]
                                    {
                                        new SyntaxTree
                                        {
                                            MetaSyntaxName = "_node-edge-query-1-OrGroup",
                                            Children = new ISyntaxTree[]
                                            {
                                                new SyntaxTree
                                                {
                                                    MetaSyntaxName = "edge-spec",
                                                    Children = new ISyntaxTree[]
                                                    {
                                                        new SyntaxPair { Token = new TokenValue("["), MetaSyntaxName = "open-bracket" },
                                                        new SyntaxTree
                                                        {
                                                            MetaSyntaxName = "tags",
                                                            Children = new ISyntaxTree[]
                                                            {
                                                                new SyntaxTree
                                                                {
                                                                    MetaSyntaxName = "tag",
                                                                    Children = new ISyntaxTree[]
                                                                    {
                                                                        new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "symbol" },
                                                                    },
                                                                },
                                                            },
                                                        },
                                                        new SyntaxPair { Token = new TokenValue("]"), MetaSyntaxName = "close-bracket" },
                                                    },
                                                },
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

        (parse.SyntaxTree == expectedTree).Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "symbol" },
            new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
            new SyntaxPair { Token = new TokenValue("->"), MetaSyntaxName = "join-left" },
            new SyntaxPair { Token = new TokenValue("["), MetaSyntaxName = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "symbol" },
            new SyntaxPair { Token = new TokenValue("]"), MetaSyntaxName = "close-bracket" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void SelectNodeToEdgeWithLabel()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("(*) -> [label]", logger);
        parse.Status.IsOk().Should().BeTrue(parse.Status.Error);

        var lines = parse.SyntaxTree.GenerateTestCodeSyntaxTree().Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "select-node-query",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxTree
                        {
                            MetaSyntaxName = "node-spec",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
                                new SyntaxTree
                                {
                                    MetaSyntaxName = "tags",
                                    Children = new ISyntaxTree[]
                                    {
                                        new SyntaxTree
                                        {
                                            MetaSyntaxName = "tag",
                                            Children = new ISyntaxTree[]
                                            {
                                                new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "symbol" },
                                            },
                                        },
                                    },
                                },
                                new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
                            },
                        },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_select-node-query-3-RepeatGroup",
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
                                                new SyntaxPair { Token = new TokenValue("->"), MetaSyntaxName = "join-left" },
                                            },
                                        },
                                    },
                                },
                                new SyntaxTree
                                {
                                    MetaSyntaxName = "node-edge-query",
                                    Children = new ISyntaxTree[]
                                    {
                                        new SyntaxTree
                                        {
                                            MetaSyntaxName = "_node-edge-query-1-OrGroup",
                                            Children = new ISyntaxTree[]
                                            {
                                                new SyntaxTree
                                                {
                                                    MetaSyntaxName = "edge-spec",
                                                    Children = new ISyntaxTree[]
                                                    {
                                                        new SyntaxPair { Token = new TokenValue("["), MetaSyntaxName = "open-bracket" },
                                                        new SyntaxTree
                                                        {
                                                            MetaSyntaxName = "tags",
                                                            Children = new ISyntaxTree[]
                                                            {
                                                                new SyntaxTree
                                                                {
                                                                    MetaSyntaxName = "tag",
                                                                    Children = new ISyntaxTree[]
                                                                    {
                                                                        new SyntaxPair { Token = new TokenValue("label"), MetaSyntaxName = "symbol" },
                                                                    },
                                                                },
                                                            },
                                                        },
                                                        new SyntaxPair { Token = new TokenValue("]"), MetaSyntaxName = "close-bracket" },
                                                    },
                                                },
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

        (parse.SyntaxTree == expectedTree).Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "symbol" },
            new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
            new SyntaxPair { Token = new TokenValue("->"), MetaSyntaxName = "join-left" },
            new SyntaxPair { Token = new TokenValue("["), MetaSyntaxName = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("label"), MetaSyntaxName = "symbol" },
            new SyntaxPair { Token = new TokenValue("]"), MetaSyntaxName = "close-bracket" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void SelectNodeToEdgeToNode()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("(key=k1) -> [label] ", logger);
        parse.Status.IsOk().Should().BeTrue(parse.Status.Error);

        var lines = parse.SyntaxTree.GenerateTestCodeSyntaxTree().Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "select-node-query",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxTree
                        {
                            MetaSyntaxName = "node-spec",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
                                new SyntaxTree
                                {
                                    MetaSyntaxName = "tags",
                                    Children = new ISyntaxTree[]
                                    {
                                        new SyntaxTree
                                        {
                                            MetaSyntaxName = "tag",
                                            Children = new ISyntaxTree[]
                                            {
                                                new SyntaxPair { Token = new TokenValue("key"), MetaSyntaxName = "symbol" },
                                                new SyntaxTree
                                                {
                                                    MetaSyntaxName = "_tag-3-OptionGroup",
                                                    Children = new ISyntaxTree[]
                                                    {
                                                        new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "_tag-3-OptionGroup-1" },
                                                        new SyntaxPair { Token = new TokenValue("k1"), MetaSyntaxName = "tagValue" },
                                                    },
                                                },
                                            },
                                        },
                                    },
                                },
                                new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
                            },
                        },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_select-node-query-3-RepeatGroup",
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
                                                new SyntaxPair { Token = new TokenValue("->"), MetaSyntaxName = "join-left" },
                                            },
                                        },
                                    },
                                },
                                new SyntaxTree
                                {
                                    MetaSyntaxName = "node-edge-query",
                                    Children = new ISyntaxTree[]
                                    {
                                        new SyntaxTree
                                        {
                                            MetaSyntaxName = "_node-edge-query-1-OrGroup",
                                            Children = new ISyntaxTree[]
                                            {
                                                new SyntaxTree
                                                {
                                                    MetaSyntaxName = "edge-spec",
                                                    Children = new ISyntaxTree[]
                                                    {
                                                        new SyntaxPair { Token = new TokenValue("["), MetaSyntaxName = "open-bracket" },
                                                        new SyntaxTree
                                                        {
                                                            MetaSyntaxName = "tags",
                                                            Children = new ISyntaxTree[]
                                                            {
                                                                new SyntaxTree
                                                                {
                                                                    MetaSyntaxName = "tag",
                                                                    Children = new ISyntaxTree[]
                                                                    {
                                                                        new SyntaxPair { Token = new TokenValue("label"), MetaSyntaxName = "symbol" },
                                                                    },
                                                                },
                                                            },
                                                        },
                                                        new SyntaxPair { Token = new TokenValue("]"), MetaSyntaxName = "close-bracket" },
                                                    },
                                                },
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

        (parse.SyntaxTree == expectedTree).Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("("), MetaSyntaxName = "open-param" },
            new SyntaxPair { Token = new TokenValue("key"), MetaSyntaxName = "symbol" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("k1"), MetaSyntaxName = "tagValue" },
            new SyntaxPair { Token = new TokenValue(")"), MetaSyntaxName = "close-param" },
            new SyntaxPair { Token = new TokenValue("->"), MetaSyntaxName = "join-left" },
            new SyntaxPair { Token = new TokenValue("["), MetaSyntaxName = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("label"), MetaSyntaxName = "symbol" },
            new SyntaxPair { Token = new TokenValue("]"), MetaSyntaxName = "close-bracket" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).Should().BeTrue();
    }


}
