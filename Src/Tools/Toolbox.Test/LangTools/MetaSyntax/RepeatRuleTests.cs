using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Test.Application;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class RepeatRuleTests : TestBase
{
    private readonly MetaSyntaxRoot _schema;

    public RepeatRuleTests(ITestOutputHelper output) : base(output)
    {
        string schemaText = new[]
{
            "symbol              = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "comma               = ',' ;",
            "tag                 = symbol, [ '=', symbol ] ;",
            "tags                = tag, { comma, tag } ;",
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().Should().BeTrue();
    }

    [Fact]
    public void SimpleRepeat()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("t1", logger);
        parse.StatusCode.IsOk().Should().BeTrue(parse.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "tag",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair
                        {
                            Token = new TokenValue("t1"),
                            MetaSyntaxName = "symbol",
                        }
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();
    }

    [Fact]
    public void SimpleTwoRepeat()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("t1, t2", logger);
        parse.StatusCode.IsOk().Should().BeTrue(parse.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "tag",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair
                        {
                            Token = new TokenValue("t1"),
                            MetaSyntaxName = "symbol",
                        },
                    },
                },
                new SyntaxTree
                {
                    MetaSyntaxName = "_tags-3-RepeatGroup",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair
                        {
                            Token = new TokenValue(","),
                            MetaSyntaxName = "comma",
                        },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "tag",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair
                                {
                                    Token = new TokenValue("t2"),
                                    MetaSyntaxName = "symbol",
                                },
                            },
                        },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();
    }

    [Fact]
    public void SimpleWithValueRepeat()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("t1=v1", logger);
        parse.StatusCode.IsOk().Should().BeTrue(parse.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "tag",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair
                        {
                            Token = new TokenValue("t1"),
                            MetaSyntaxName = "symbol",
                        },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_tag-3-OptionGroup",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair
                                {
                                    Token = new TokenValue("="),
                                    MetaSyntaxName = "_tag-3-OptionGroup-1",
                                },
                                new SyntaxPair
                                {
                                    Token = new TokenValue("v1"),
                                    MetaSyntaxName = "symbol",
                                },
                            },
                        },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();
    }

    [Fact]
    public void SimpleTwoWithValueRepeat()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("t1=v1, t2", logger);
        parse.StatusCode.IsOk().Should().BeTrue(parse.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "tag",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair
                        {
                            Token = new TokenValue("t1"),
                            MetaSyntaxName = "symbol",
                        },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_tag-3-OptionGroup",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair
                                {
                                    Token = new TokenValue("="),
                                    MetaSyntaxName = "_tag-3-OptionGroup-1",
                                },
                                new SyntaxPair
                                {
                                    Token = new TokenValue("v1"),
                                    MetaSyntaxName = "symbol",
                                },
                            },
                        },
                    },
                },
                new SyntaxTree
                {
                    MetaSyntaxName = "_tags-3-RepeatGroup",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair
                        {
                            Token = new TokenValue(","),
                            MetaSyntaxName = "comma",
                        },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "tag",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair
                                {
                                    Token = new TokenValue("t2"),
                                    MetaSyntaxName = "symbol",
                                },
                            },
                        },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();
    }

    [Fact]
    public void SimpleTwoWithTwoValueRepeat()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("t1=v1, t2=v2", logger);
        parse.StatusCode.IsOk().Should().BeTrue(parse.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "tag",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair
                        {
                            Token = new TokenValue("t1"),
                            MetaSyntaxName = "symbol",
                        },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_tag-3-OptionGroup",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair
                                {
                                    Token = new TokenValue("="),
                                    MetaSyntaxName = "_tag-3-OptionGroup-1",
                                },
                                new SyntaxPair
                                {
                                    Token = new TokenValue("v1"),
                                    MetaSyntaxName = "symbol",
                                },
                            },
                        },
                    },
                },
                new SyntaxTree
                {
                    MetaSyntaxName = "_tags-3-RepeatGroup",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair
                        {
                            Token = new TokenValue(","),
                            MetaSyntaxName = "comma",
                        },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "tag",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair
                                {
                                    Token = new TokenValue("t2"),
                                    MetaSyntaxName = "symbol",
                                },
                                new SyntaxTree
                                {
                                    MetaSyntaxName = "_tag-3-OptionGroup",
                                    Children = new ISyntaxTree[]
                                    {
                                        new SyntaxPair
                                        {
                                            Token = new TokenValue("="),
                                            MetaSyntaxName = "_tag-3-OptionGroup-1",
                                        },
                                        new SyntaxPair
                                        {
                                            Token = new TokenValue("v2"),
                                            MetaSyntaxName = "symbol",
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
    }

    [Fact]
    public void ThreeTagsRepeat()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("t1=v1, t2=v2, t3=v3", logger);
        parse.StatusCode.IsOk().Should().BeTrue(parse.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "tag",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair
                        {
                            Token = new TokenValue("t1"),
                            MetaSyntaxName = "symbol",
                        },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_tag-3-OptionGroup",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair
                                {
                                    Token = new TokenValue("="),
                                    MetaSyntaxName = "_tag-3-OptionGroup-1",
                                },
                                new SyntaxPair
                                {
                                    Token = new TokenValue("v1"),
                                    MetaSyntaxName = "symbol",
                                },
                            },
                        },
                    },
                },
                new SyntaxTree
                {
                    MetaSyntaxName = "_tags-3-RepeatGroup",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair
                        {
                            Token = new TokenValue(","),
                            MetaSyntaxName = "comma",
                        },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "tag",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair
                                {
                                    Token = new TokenValue("t2"),
                                    MetaSyntaxName = "symbol",
                                },
                                new SyntaxTree
                                {
                                    MetaSyntaxName = "_tag-3-OptionGroup",
                                    Children = new ISyntaxTree[]
                                    {
                                        new SyntaxPair
                                        {
                                            Token = new TokenValue("="),
                                            MetaSyntaxName = "_tag-3-OptionGroup-1",
                                        },
                                        new SyntaxPair
                                        {
                                            Token = new TokenValue("v2"),
                                            MetaSyntaxName = "symbol",
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
                new SyntaxTree
                {
                    MetaSyntaxName = "_tags-3-RepeatGroup",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair
                        {
                            Token = new TokenValue(","),
                            MetaSyntaxName = "comma",
                        },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "tag",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair
                                {
                                    Token = new TokenValue("t3"),
                                    MetaSyntaxName = "symbol",
                                },
                                new SyntaxTree
                                {
                                    MetaSyntaxName = "_tag-3-OptionGroup",
                                    Children = new ISyntaxTree[]
                                    {
                                        new SyntaxPair
                                        {
                                            Token = new TokenValue("="),
                                            MetaSyntaxName = "_tag-3-OptionGroup-1",
                                        },
                                        new SyntaxPair
                                        {
                                            Token = new TokenValue("v3"),
                                            MetaSyntaxName = "symbol",
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
    }
}
