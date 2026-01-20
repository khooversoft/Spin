using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax.GraphModel;

public class SelectEdgeQueryTests
{
    private readonly MetaSyntaxRoot _schema;
    private readonly IHost _host;

    public SelectEdgeQueryTests(ITestOutputHelper output)
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
            "select-edge-query   = edge-spec, { join, node-edge-query } ;",
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().BeTrue(_schema.Error);

        _host = Host.CreateDefaultBuilder()
            .AddDebugLogging(x => output.WriteLine(x))
            .Build();

    }

    [Theory]
    [InlineData("")]
    [InlineData("[]")]
    [InlineData("[*])")]
    [InlineData("[!*]")]
    public void FailedReturn(string command)
    {
        var parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);

        var parse = parser.Parse(command);
        parse.Status.IsError().BeTrue(parse.Status.Error);
    }


    [Fact]
    public void SelectEdge()
    {
        var parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);

        var parse = parser.Parse("[*]");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = parse.SyntaxTree.GenerateTestCodeSyntaxTree().Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "select-edge-query",
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
                },
            },
        };

        (parse.SyntaxTree == expectedTree).BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("["), Name = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("]"), Name = "close-bracket" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }

    [Fact]
    public void SelectEdgeToNode()
    {
        var parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);

        var parse = parser.Parse("[*] -> (*)");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = parse.SyntaxTree.GenerateTestCodeSyntaxTree().Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "select-edge-query",
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
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_select-edge-query-3-RepeatGroup",
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
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("["), Name = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("]"), Name = "close-bracket" },
            new SyntaxPair { Token = new TokenValue("->"), Name = "join-left" },
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }

    [Fact]
    public void SelectNodeToEdgeWithLabel()
    {
        var parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);

        var parse = parser.Parse("[label] -> (*)");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = parse.SyntaxTree.GenerateTestCodeSyntaxTree().Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "select-edge-query",
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
                                                new SyntaxPair { Token = new TokenValue("label"), Name = "symbol" },
                                            },
                                        },
                                    },
                                },
                                new SyntaxPair { Token = new TokenValue("]"), Name = "close-bracket" },
                            },
                        },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_select-edge-query-3-RepeatGroup",
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
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("["), Name = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("label"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("]"), Name = "close-bracket" },
            new SyntaxPair { Token = new TokenValue("->"), Name = "join-left" },
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("*"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }

    [Fact]
    public void SelectNodeToEdgeToNode()
    {
        var parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);

        var parse = parser.Parse("[label] -> (key=k1)");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = parse.SyntaxTree.GenerateTestCodeSyntaxTree().Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "select-edge-query",
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
                                                new SyntaxPair { Token = new TokenValue("label"), Name = "symbol" },
                                            },
                                        },
                                    },
                                },
                                new SyntaxPair { Token = new TokenValue("]"), Name = "close-bracket" },
                            },
                        },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_select-edge-query-3-RepeatGroup",
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
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("["), Name = "open-bracket" },
            new SyntaxPair { Token = new TokenValue("label"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("]"), Name = "close-bracket" },
            new SyntaxPair { Token = new TokenValue("->"), Name = "join-left" },
            new SyntaxPair { Token = new TokenValue("("), Name = "open-param" },
            new SyntaxPair { Token = new TokenValue("key"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("k1"), Name = "tagValue" },
            new SyntaxPair { Token = new TokenValue(")"), Name = "close-param" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }
}
