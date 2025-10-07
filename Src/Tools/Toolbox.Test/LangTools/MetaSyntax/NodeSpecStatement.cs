﻿using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Test.Application;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class NodeSpecStatement : TestBase
{
    private readonly MetaSyntaxRoot _schema;

    public NodeSpecStatement(ITestOutputHelper output) : base(output)
    {
        string schemaText = new[]
{
            "delimiters          = , [ ] = { } ( ) ;",
            "symbol              = regex '^[a-zA-Z\\*][a-zA-Z0-9\\-\\*]*$' ;",
            "alias               = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "tagValue            = string ;",
            "comma               = ',' ;",
            "tag                 = symbol, [ '=', tagValue ] ;",
            "tags                = tag, { comma, tag } ;",
            "open-param          = '(' #group-start #node;",
            "close-param         = ')' #group-end #node ;",
            "node-spec           = open-param, tags, close-param, [ alias ] ;",
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().BeTrue(_schema.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("()")]
    public void FailedReturn(string command)
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse(command, logger);
        parse.Status.IsError().BeTrue(parse.Status.Error);
    }

    [Fact]
    public void WildcardSearchOfNode()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("(*)", logger);
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "node-spec",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
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
                                        new SyntaxPair { Token = new TokenValue("*"), Name = "symbol" },
                                    },
                                },
                            },
                        },
                        new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void WildcardSearchOfNodeWithAlias()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("(*) a1", logger);
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "node-spec",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
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
                                        new SyntaxPair { Token = new TokenValue("*"), Name = "symbol" },
                                    },
                                },
                            },
                        },
                        new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_node-spec-7-OptionGroup",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair { Token = new TokenValue("a1"), Name = "alias" },
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
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
            new SyntaxPair { Token = new TokenValue("a1"), Name = "alias" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void FilterOnTag()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("(t1)", logger);
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "node-spec",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
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
                                        new SyntaxPair { Token = new TokenValue("t1"), Name = "symbol" },
                                    },
                                },
                            },
                        },
                        new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("t1"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void FilterOnNodeKey()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("(key=k1)", logger);
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "node-spec",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
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
                                        new SyntaxPair { Token = new TokenValue("key"), Name = "symbol" },
                                        new SyntaxTree
                                        {
                                            MetaSyntaxName = "_tag-3-OptionGroup",
                                            Children = new ISyntaxTree[]
                                            {
                                                new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
                                                new SyntaxPair { Token = new TokenValue("k1"), Name = "tagValue" },
                                            },
                                        },
                                    },
                                },
                            },
                        },
                        new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("key"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("k1"), Name = "tagValue" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void FilterOnNodeKeyAndTag()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("(key=k1, t2) a2", logger);
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "node-spec",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
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
                                        new SyntaxPair { Token = new TokenValue("key"), Name = "symbol" },
                                        new SyntaxTree
                                        {
                                            MetaSyntaxName = "_tag-3-OptionGroup",
                                            Children = new ISyntaxTree[]
                                            {
                                                new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
                                                new SyntaxPair { Token = new TokenValue("k1"), Name = "tagValue" },
                                            },
                                        },
                                    },
                                },
                                new SyntaxTree
                                {
                                    MetaSyntaxName = "_tags-3-RepeatGroup",
                                    Children = new ISyntaxTree[]
                                    {
                                        new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
                                        new SyntaxTree
                                        {
                                            MetaSyntaxName = "tag",
                                            Children = new ISyntaxTree[]
                                            {
                                                new SyntaxPair { Token = new TokenValue("t2"), Name = "symbol" },
                                            },
                                        },
                                    },
                                },
                            },
                        },
                        new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_node-spec-7-OptionGroup",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair { Token = new TokenValue("a2"), Name = "alias" },
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
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("key"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("k1"), Name = "tagValue" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("t2"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
            new SyntaxPair { Token = new TokenValue("a2"), Name = "alias" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }
}
