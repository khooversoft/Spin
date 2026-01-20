using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class EdgeSpecStatement
{
    private readonly MetaSyntaxRoot _schema;
    private readonly IHost _host;
    private readonly SyntaxParser _parser;

    public EdgeSpecStatement(ITestOutputHelper output)
    {
        string schemaText = new[]
{
            "delimiters          = , [ ] { } = ;",
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
        _schema.StatusCode.IsOk().BeTrue(_schema.Error);

        _host = Host.CreateDefaultBuilder()
            .AddDebugLogging(x => output.WriteLine(x))
            .Build();

        _parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);
    }

    [Theory]
    [InlineData("")]
    [InlineData("[]")]
    [InlineData("[*])")]
    [InlineData("[!*]")]
    public void FailedReturn(string command)
    {
        var parse = _parser.Parse(command);
        parse.Status.IsError().BeTrue(parse.Status.Error);
    }

    [Fact]
    public void WildcardSearchOfNode()
    {
        var parse = _parser.Parse("[*]");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "edge-spec",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("["), Name = "open-bracket" },
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
                        new SyntaxPair { Token = new TokenValue("]"), Name = "close-bracket" },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("["), Name = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("]"), Name = "close-bracket" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void WildcardSearchOfNodeWithAlias()
    {
        var parse = _parser.Parse("[*] a1");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "edge-spec",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("["), Name = "open-bracket" },
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
                        new SyntaxPair { Token = new TokenValue("]"), Name = "close-bracket" },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_edge-spec-7-OptionGroup",
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
            new SyntaxPair { Token = new TokenValue("["), Name = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("]"), Name = "close-bracket" },
            new SyntaxPair { Token = new TokenValue("a1"), Name = "alias" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void FilterOnTag()
    {
        var parse = _parser.Parse("[t1]");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "edge-spec",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("["), Name = "open-bracket" },
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
                        new SyntaxPair { Token = new TokenValue("]"), Name = "close-bracket" },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("["), Name = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("t1"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("]"), Name = "close-bracket" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void FilterOnNodeKey()
    {
        var parse = _parser.Parse("[ key = k1]");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "edge-spec",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("["), Name = "open-bracket" },
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
                        new SyntaxPair { Token = new TokenValue("]"), Name = "close-bracket" },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("["), Name = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("key"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("k1"), Name = "tagValue" },
            new SyntaxPair { Token = new TokenValue("]"), Name = "close-bracket" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void FilterOnNodeKeyAndTag()
    {
        var parse = _parser.Parse("[ key = k1, t2] a2");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "edge-spec",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("["), Name = "open-bracket" },
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
                        new SyntaxPair { Token = new TokenValue("]"), Name = "close-bracket" },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_edge-spec-7-OptionGroup",
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
            new SyntaxPair { Token = new TokenValue("["), Name = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("key"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("k1"), Name = "tagValue" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("t2"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("]"), Name = "close-bracket" },
            new SyntaxPair { Token = new TokenValue("a2"), Name = "alias" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }
}
