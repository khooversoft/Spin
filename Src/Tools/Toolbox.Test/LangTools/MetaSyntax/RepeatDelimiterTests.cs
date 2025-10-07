using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Test.Application;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class RepeatDelimiterTests : TestBase
{
    private readonly MetaSyntaxRoot _schema;

    public RepeatDelimiterTests(ITestOutputHelper output) : base(output)
    {
        string schemaText = new[]
        {
            "delimiters          = , [ ] = { } ( ) ;",
            "symbol              = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "tagValue            = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "name                = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "comma               = ',' ;",
            "open-brace          = '{' ;",
            "close-brace         = '}' ;",
            "open-param          = '(' #group-start #node;",
            "close-param         = ')' #group-end #node ;",
            "base64              = string ;",
            "term                = ';' ;",
            "tag                 = symbol, [ '=', tagValue ] ;",
            "tags                = tag, { comma, tag } ;",
            "entity-data         = name, open-brace, base64, close-brace ;",
            "node-spec           = open-param, tags, close-param ;",
            "select              = node-spec, [ entity-data ], term ;",
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("()")]
    [InlineData("();")]
    [InlineData("data { hexdata };")]
    public void FailedReturn(string command)
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse(command, logger);
        parse.Status.IsError().BeTrue(parse.Status.Error);
    }

    [Fact]
    public void SingleTag()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("(t1) ;", logger);
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
                        new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
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
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void SingleTagWithData()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("(key=k1) data { 'data section' } ;", logger);
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
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_select-3-OptionGroup",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxTree
                                {
                                    MetaSyntaxName = "entity-data",
                                    Children = new ISyntaxTree[]
                                    {
                                        new SyntaxPair { Token = new TokenValue("data"), Name = "name" },
                                        new SyntaxPair { Token = new TokenValue("{"), Name = "open-brace" },
                                        new SyntaxPair { Token = new BlockToken("'data section'", '\'', '\'', 16), Name = "base64" },
                                        new SyntaxPair { Token = new TokenValue("}"), Name = "close-brace" },
                                    },
                                },
                            },
                        },
                        new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
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
            new SyntaxPair { Token = new TokenValue("data"), Name = "name" },
            new SyntaxPair { Token = new TokenValue("{"), Name = "open-brace" },
            new SyntaxPair { Token = new BlockToken("'data section'", '\'', '\'', 16), Name = "base64" },
            new SyntaxPair { Token = new TokenValue("}"), Name = "close-brace" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }
}
