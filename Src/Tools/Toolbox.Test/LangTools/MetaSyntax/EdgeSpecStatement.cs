using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Types;
using Toolbox.Test.Application;
using Xunit.Abstractions;
using FluentAssertions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class EdgeSpecStatement : TestBase
{
    private readonly MetaSyntaxRoot _schema;

    public EdgeSpecStatement(ITestOutputHelper output) : base(output)
    {
        string schemaText = new[]
{
            "symbol              = regex '^[a-zA-Z\\*][a-zA-Z0-9\\-\\*]*$' ;",
            "alias               = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "tagValue            = string ;",
            "comma               = ',' ;",
            "tag                 = symbol, [ '=', tagValue ] ;",
            "tags                = tag, { comma, tag } ;",
            "open-bracket        = '[' #group-start #edge ;",
            "close-bracket       = ']' #group-end #edge ;",
            "edge-spec           = open-bracket, tags, close-bracket, [ alias ] ;",
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
        parse.StatusCode.IsError().Should().BeTrue(parse.Error);
    }

    [Fact]
    public void WildcardSearchOfNode()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("[*]", logger);
        parse.StatusCode.IsOk().Should().BeTrue(parse.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
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
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("["), MetaSyntaxName = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "symbol" },
            new SyntaxPair { Token = new TokenValue("]"), MetaSyntaxName = "close-bracket" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void WildcardSearchOfNodeWithAlias()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("[*] a1", logger);
        parse.StatusCode.IsOk().Should().BeTrue(parse.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
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
                new SyntaxTree
                {
                    MetaSyntaxName = "_edge-spec-7-OptionGroup",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("a1"), MetaSyntaxName = "alias" },
                    },
                },
            },
                };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("["), MetaSyntaxName = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("*"), MetaSyntaxName = "symbol" },
            new SyntaxPair { Token = new TokenValue("]"), MetaSyntaxName = "close-bracket" },
            new SyntaxPair { Token = new TokenValue("a1"), MetaSyntaxName = "alias" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void FilterOnTag()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("[t1]", logger);
        parse.StatusCode.IsOk().Should().BeTrue(parse.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
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
                                new SyntaxPair { Token = new TokenValue("t1"), MetaSyntaxName = "symbol" },
                            },
                        },
                    },
                },
                new SyntaxPair { Token = new TokenValue("]"), MetaSyntaxName = "close-bracket" },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("["), MetaSyntaxName = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("t1"), MetaSyntaxName = "symbol" },
            new SyntaxPair { Token = new TokenValue("]"), MetaSyntaxName = "close-bracket" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void FilterOnNodeKey()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("[ key = k1]", logger);
        parse.StatusCode.IsOk().Should().BeTrue(parse.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
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
                new SyntaxPair { Token = new TokenValue("]"), MetaSyntaxName = "close-bracket" },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("["), MetaSyntaxName = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("key"), MetaSyntaxName = "symbol" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("k1"), MetaSyntaxName = "tagValue" },
            new SyntaxPair { Token = new TokenValue("]"), MetaSyntaxName = "close-bracket" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).Should().BeTrue();
    }

    [Fact]
    public void FilterOnNodeKeyAndTag()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("[ key = k1, t2] a2", logger);
        parse.StatusCode.IsOk().Should().BeTrue(parse.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
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
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_tags-3-RepeatGroup",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
                                new SyntaxTree
                                {
                                    MetaSyntaxName = "tag",
                                    Children = new ISyntaxTree[]
                                    {
                                        new SyntaxPair { Token = new TokenValue("t2"), MetaSyntaxName = "symbol" },
                                    },
                                },
                            },
                        },
                    },
                },
                new SyntaxPair { Token = new TokenValue("]"), MetaSyntaxName = "close-bracket" },
                new SyntaxTree
                {
                    MetaSyntaxName = "_edge-spec-7-OptionGroup",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("a2"), MetaSyntaxName = "alias" },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("["), MetaSyntaxName = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("key"), MetaSyntaxName = "symbol" },
            new SyntaxPair { Token = new TokenValue("="), MetaSyntaxName = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("k1"), MetaSyntaxName = "tagValue" },
            new SyntaxPair { Token = new TokenValue(","), MetaSyntaxName = "comma" },
            new SyntaxPair { Token = new TokenValue("t2"), MetaSyntaxName = "symbol" },
            new SyntaxPair { Token = new TokenValue("]"), MetaSyntaxName = "close-bracket" },
            new SyntaxPair { Token = new TokenValue("a2"), MetaSyntaxName = "alias" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).Should().BeTrue();
    }
}
