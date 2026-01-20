using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class RepeatRuleTests
{
    private readonly MetaSyntaxRoot _schema;
    private readonly IHost _host;
    private readonly SyntaxParser _parser;

    public RepeatRuleTests(ITestOutputHelper output)
    {
        string schemaText = new[]
        {
            "delimiters          = , [ ] = { } ;",
            "symbol              = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "tagValue            = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "comma               = ',' ;",
            "tag                 = symbol, [ '=', tagValue ] ;",
            "tags                = tag, { comma, tag } ;",
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().BeTrue();

        _host = Host.CreateDefaultBuilder()
            .AddDebugLogging(x => output.WriteLine(x))
            .Build();

        _parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);
    }

    [Fact]
    public void SimpleRepeat()
    {
        var parse = _parser.Parse("t1");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
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
            },
        };

        (parse.SyntaxTree == expectedTree).BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("t1"), Name = "symbol" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void SimpleTwoRepeat()
    {
        var parse = _parser.Parse("t1, t2");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
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
            },
        };

        (parse.SyntaxTree == expectedTree).BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("t1"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("t2"), Name = "symbol" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void SimpleWithValueRepeat()
    {
        var parse = _parser.Parse("t1=v1");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
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
                                new SyntaxTree
                                {
                                    MetaSyntaxName = "_tag-3-OptionGroup",
                                    Children = new ISyntaxTree[]
                                    {
                                        new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
                                        new SyntaxPair { Token = new TokenValue("v1"), Name = "tagValue" },
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
            new SyntaxPair { Token = new TokenValue("t1"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("v1"), Name = "tagValue" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void SimpleTwoWithValueRepeat()
    {
        var parse = _parser.Parse("t1=v1, t2");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
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
                                new SyntaxTree
                                {
                                    MetaSyntaxName = "_tag-3-OptionGroup",
                                    Children = new ISyntaxTree[]
                                    {
                                        new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
                                        new SyntaxPair { Token = new TokenValue("v1"), Name = "tagValue" },
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
            },
        };

        (parse.SyntaxTree == expectedTree).BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("t1"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("v1"), Name = "tagValue" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("t2"), Name = "symbol" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void SimpleTwoWithTwoValueRepeat()
    {
        var parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);

        var parse = parser.Parse("t1=v1, t2=v2");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
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
                                new SyntaxTree
                                {
                                    MetaSyntaxName = "_tag-3-OptionGroup",
                                    Children = new ISyntaxTree[]
                                    {
                                        new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
                                        new SyntaxPair { Token = new TokenValue("v1"), Name = "tagValue" },
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
                                        new SyntaxTree
                                        {
                                            MetaSyntaxName = "_tag-3-OptionGroup",
                                            Children = new ISyntaxTree[]
                                            {
                                                new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
                                                new SyntaxPair { Token = new TokenValue("v2"), Name = "tagValue" },
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
            new SyntaxPair { Token = new TokenValue("t1"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("v1"), Name = "tagValue" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("t2"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("v2"), Name = "tagValue" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void ThreeTagsRepeat()
    {
        var parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);

        var parse = parser.Parse("t1=v1, t2=v2, t3=v3");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
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
                                new SyntaxTree
                                {
                                    MetaSyntaxName = "_tag-3-OptionGroup",
                                    Children = new ISyntaxTree[]
                                    {
                                        new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
                                        new SyntaxPair { Token = new TokenValue("v1"), Name = "tagValue" },
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
                                        new SyntaxTree
                                        {
                                            MetaSyntaxName = "_tag-3-OptionGroup",
                                            Children = new ISyntaxTree[]
                                            {
                                                new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
                                                new SyntaxPair { Token = new TokenValue("v2"), Name = "tagValue" },
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
                                new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
                                new SyntaxTree
                                {
                                    MetaSyntaxName = "tag",
                                    Children = new ISyntaxTree[]
                                    {
                                        new SyntaxPair { Token = new TokenValue("t3"), Name = "symbol" },
                                        new SyntaxTree
                                        {
                                            MetaSyntaxName = "_tag-3-OptionGroup",
                                            Children = new ISyntaxTree[]
                                            {
                                                new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
                                                new SyntaxPair { Token = new TokenValue("v3"), Name = "tagValue" },
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
            new SyntaxPair { Token = new TokenValue("t1"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("v1"), Name = "tagValue" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("t2"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("v2"), Name = "tagValue" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("t3"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("v3"), Name = "tagValue" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void ThreeTagsRepeatWithOneNoValue()
    {
        var parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);

        var parse = parser.Parse("t1=v1, t2, t3=v3");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
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
                                new SyntaxTree
                                {
                                    MetaSyntaxName = "_tag-3-OptionGroup",
                                    Children = new ISyntaxTree[]
                                    {
                                        new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
                                        new SyntaxPair { Token = new TokenValue("v1"), Name = "tagValue" },
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
                                        new SyntaxPair { Token = new TokenValue("t3"), Name = "symbol" },
                                        new SyntaxTree
                                        {
                                            MetaSyntaxName = "_tag-3-OptionGroup",
                                            Children = new ISyntaxTree[]
                                            {
                                                new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
                                                new SyntaxPair { Token = new TokenValue("v3"), Name = "tagValue" },
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
            new SyntaxPair { Token = new TokenValue("t1"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("v1"), Name = "tagValue" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("t2"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("t3"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("v3"), Name = "tagValue" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }
}
