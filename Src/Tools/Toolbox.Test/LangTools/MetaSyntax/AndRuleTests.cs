using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Types;
using Toolbox.LangTools;
using Toolbox.Test.Application;
using Xunit.Abstractions;
using FluentAssertions;
using Toolbox.Tools;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class AndRuleTests : TestBase
{
    private readonly MetaSyntaxRoot _schema;

    public AndRuleTests(ITestOutputHelper output) : base(output)
    {
        string schemaText = new[]
{
            "symbol              = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "open-brace          = '{' #group-start #data ;",
            "close-brace         = '}' #group-end #data ;",
            "base64              = string ;",
            "term                = ';' ;",
            "entity-data         = symbol, open-brace, base64, close-brace, term;",
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().Should().BeTrue();
    }

    [Theory]
    [InlineData("alias { hello }")]
    [InlineData("alias  hello };")]
    [InlineData("alias { hello ;")]
    [InlineData("alias { };")]
    [InlineData("{ hello };")]
    public void SimpleAndSymbolFail(string rawData)
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        parser.Parse(rawData, logger).StatusCode.IsError().Should().BeTrue();
    }

    [Fact]
    public void SimpleAndSymbol()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("alias { hello };", logger);
        parse.StatusCode.IsOk().Should().BeTrue();

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxPair
                {
                    Token = new TokenValue("alias"),
                    MetaSyntax = new TerminalSymbol { Name = "symbol", Text = "^[a-zA-Z][a-zA-Z0-9\\-]*$", Type = TerminalType.Regex },
                },
                new SyntaxPair
                {
                    Token = new TokenValue("{"),
                    MetaSyntax = new TerminalSymbol { Name = "open-brace", Text = "{", Type = TerminalType.Token, Tags = ["group-start","data"] },
                },
                new SyntaxPair
                {
                    Token = new TokenValue("hello"),
                    MetaSyntax = new TerminalSymbol { Name = "base64", Text = "string", Type = TerminalType.String },
                },
                new SyntaxPair
                {
                    Token = new TokenValue("}"),
                    MetaSyntax = new TerminalSymbol { Name = "close-brace", Text = "}", Type = TerminalType.Token, Tags = ["group-end","data"] },
                },
                new SyntaxPair
                {
                    Token = new TokenValue(";"),
                    MetaSyntax = new TerminalSymbol { Name = "term", Text = ";", Type = TerminalType.Token },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();
    }

    [Fact]
    public void SimpleAndSymbolWithQuote()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("data { 'this is a test' };", logger);
        parse.StatusCode.IsOk().Should().BeTrue(parse.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxPair
                {
                    Token = new TokenValue("data"),
                    MetaSyntax = new TerminalSymbol { Name = "symbol", Text = "^[a-zA-Z][a-zA-Z0-9\\-]*$", Type = TerminalType.Regex },
                },
                new SyntaxPair
                {
                    Token = new TokenValue("{"),
                    MetaSyntax = new TerminalSymbol { Name = "open-brace", Text = "{", Type = TerminalType.Token, Tags = ["group-start","data"] },
                },
                new SyntaxPair
                {
                    Token = new BlockToken("'this is a test'", '\'', '\'', 7),
                    MetaSyntax = new TerminalSymbol { Name = "base64", Text = "string", Type = TerminalType.String },
                },
                new SyntaxPair
                {
                    Token = new TokenValue("}"),
                    MetaSyntax = new TerminalSymbol { Name = "close-brace", Text = "}", Type = TerminalType.Token, Tags = ["group-end","data"] },
                },
                new SyntaxPair
                {
                    Token = new TokenValue(";"),
                    MetaSyntax = new TerminalSymbol { Name = "term", Text = ";", Type = TerminalType.Token },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();
    }
}
