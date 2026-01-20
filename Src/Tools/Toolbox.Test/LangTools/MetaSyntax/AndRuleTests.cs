using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class AndRuleTests
{
    private readonly MetaSyntaxRoot _schema;
    private readonly IHost _host;
    private readonly SyntaxParser _parser;

    public AndRuleTests(ITestOutputHelper output)
    {
        string schemaText = new[]
        {
            "delimiters          = { } ';' ;",
            "symbol              = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "open-brace          = '{' #group-start #data ;",
            "close-brace         = '}' #group-end #data ;",
            "base64              = string ;",
            "term                = ';' ;",
            "entity-data         = symbol, open-brace, base64, close-brace, term;",
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().BeTrue();

        _host = Host.CreateDefaultBuilder()
            .AddDebugLogging(x => output.WriteLine(x))
            .Build();

        _parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);
    }

    [Theory]
    [InlineData("alias { hello }")]
    [InlineData("alias  hello };")]
    [InlineData("alias { hello ;")]
    [InlineData("alias { };")]
    [InlineData("{ hello };")]
    public void SimpleAndSymbolFail(string rawData)
    {
        _parser.Parse(rawData).Status.IsError().BeTrue();
    }

    [Fact]
    public void SimpleAndSymbol()
    {
        var parse = _parser.Parse("alias { hello };");
        parse.Status.IsOk().BeTrue();

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "entity-data",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("alias"), Name = "symbol" },
                        new SyntaxPair { Token = new TokenValue("{"), Name = "open-brace" },
                        new SyntaxPair { Token = new TokenValue("hello"), Name = "base64" },
                        new SyntaxPair { Token = new TokenValue("}"), Name = "close-brace" },
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
            new SyntaxPair { Token = new TokenValue("alias"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("{"), Name = "open-brace" },
            new SyntaxPair { Token = new TokenValue("hello"), Name = "base64" },
            new SyntaxPair { Token = new TokenValue("}"), Name = "close-brace" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void SimpleAndSymbolWithQuote()
    {
        var parse = _parser.Parse("data { 'this is a test' };");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "entity-data",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("data"), Name = "symbol" },
                        new SyntaxPair { Token = new TokenValue("{"), Name = "open-brace" },
                        new SyntaxPair { Token = new BlockToken("'this is a test'", '\'', '\'', 7), Name = "base64" },
                        new SyntaxPair { Token = new TokenValue("}"), Name = "close-brace" },
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
            new SyntaxPair { Token = new TokenValue("data"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue("{"), Name = "open-brace" },
            new SyntaxPair { Token = new BlockToken("'this is a test'", '\'', '\'', 7), Name = "base64" },
            new SyntaxPair { Token = new TokenValue("}"), Name = "close-brace" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }
}
