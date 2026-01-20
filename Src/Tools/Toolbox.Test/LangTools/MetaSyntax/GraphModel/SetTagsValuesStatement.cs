using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax.GraphModel;

public class SetTagsValuesStatement
{
    private readonly MetaSyntaxRoot _schema;
    private readonly IHost _host;

    public SetTagsValuesStatement(ITestOutputHelper output)
    {
        string schemaText = new[]
        {
            "delimiters          = [ = ] , { } ;",
            "symbol              = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "tagValue            = string ;",
            "tag                 = symbol, [ '=', tagValue ] ;",
            "comma               = ',' ;",
            "tags                = tag, { comma, tag } ;",
            "set-sym             = 'set' ;",
            "set-cmd             = set-sym, tags ;",
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().BeTrue(_schema.Error);

        _host = Host.CreateDefaultBuilder()
            .AddDebugLogging(x => output.WriteLine(x))
            .Build();
    }

    [Theory]
    [InlineData("")]
    [InlineData("set")]
    [InlineData("{ base64data }")]
    public void FailedReturn(string command)
    {
        var parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);

        var parse = parser.Parse(command);
        parse.Status.IsError().BeTrue(parse.Status.Error);
    }

    [Fact]
    public void SingleSet()
    {
        var parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);

        var parse = parser.Parse("set t1");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = parse.SyntaxTree.GenerateTestCodeSyntaxTree().Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "set-cmd",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("set"), Name = "set-sym" },
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
                },
            },
        };

        (parse.SyntaxTree == expectedTree).BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("set"), Name = "set-sym" },
            new SyntaxPair { Token = new TokenValue("t1"), Name = "symbol" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }


    [Fact]
    public void TwoTagsSet()
    {
        var parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);

        var parse = parser.Parse("set t1, t2");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = parse.SyntaxTree.GenerateTestCodeSyntaxTree().Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "set-cmd",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("set"), Name = "set-sym" },
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
                },
            },
        };

        (parse.SyntaxTree == expectedTree).BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("set"), Name = "set-sym" },
            new SyntaxPair { Token = new TokenValue("t1"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("t2"), Name = "symbol" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }


    [Fact]
    public void TwoValuesSet()
    {
        var parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);

        var parse = parser.Parse("set t1=v1, t2, t3=v3");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = parse.SyntaxTree.GenerateTestCodeSyntaxTree().Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "set-cmd",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("set"), Name = "set-sym" },
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
                },
            },
        };

        (parse.SyntaxTree == expectedTree).BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("set"), Name = "set-sym" },
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

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }
}
