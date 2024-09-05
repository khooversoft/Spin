﻿using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Test.Application;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class SetDataStatement : TestBase
{
    private readonly MetaSyntaxRoot _schema;

    public SetDataStatement(ITestOutputHelper output) : base(output)
    {
        string schemaText = new[]
        {
            "delimiters          = { } ;",
            "symbol              = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "base64              = string ;",
            "open-brace          = '{' #group-start #data ;",
            "close-brace         = '}' #group-end #data ;",
            "entity-data         = symbol, open-brace, base64, close-brace ;",
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().Should().BeTrue();
    }


    [Theory]
    [InlineData("")]
    [InlineData("data { base64data } ;")]
    [InlineData("data { base64data }, data { newBase64 }")]
    [InlineData("data { }")]
    [InlineData("data base64data }")]
    [InlineData("data { base64data")]
    [InlineData("{ base64data }")]
    public void FailedReturn(string command)
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse(command, logger);
        parse.Status.IsError().Should().BeTrue(parse.Status.Error);
    }

    [Fact]
    public void SingleSetData()
    {
        var parser = new SyntaxParser(_schema);
        var logger = GetScopeContext<OrRuleTests>();

        var parse = parser.Parse("data { base64data }", logger);
        parse.Status.IsOk().Should().BeTrue(parse.Status.Error);

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
                        new SyntaxPair { Token = new TokenValue("data"), MetaSyntaxName = "symbol" },
                        new SyntaxPair { Token = new TokenValue("{"), MetaSyntaxName = "open-brace" },
                        new SyntaxPair { Token = new TokenValue("base64data"), MetaSyntaxName = "base64" },
                        new SyntaxPair { Token = new TokenValue("}"), MetaSyntaxName = "close-brace" },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("data"), MetaSyntaxName = "symbol" },
            new SyntaxPair { Token = new TokenValue("{"), MetaSyntaxName = "open-brace" },
            new SyntaxPair { Token = new TokenValue("base64data"), MetaSyntaxName = "base64" },
            new SyntaxPair { Token = new TokenValue("}"), MetaSyntaxName = "close-brace" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).Should().BeTrue();
    }
}
