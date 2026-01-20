using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class SetDataStatement
{
    private readonly MetaSyntaxRoot _schema;
    private readonly IHost _host;
    private readonly SyntaxParser _parser;

    public SetDataStatement(ITestOutputHelper output)
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
        _schema.StatusCode.IsOk().BeTrue();

        _host = Host.CreateDefaultBuilder()
            .AddDebugLogging(x => output.WriteLine(x))
            .Build();

        _parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);
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
        var parse = _parser.Parse(command);
        parse.Status.IsError().BeTrue(parse.Status.Error);
    }

    [Fact]
    public void SingleSetData()
    {
        var parse = _parser.Parse("data { base64data }");
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
                        new SyntaxPair { Token = new TokenValue("base64data"), Name = "base64" },
                        new SyntaxPair { Token = new TokenValue("}"), Name = "close-brace" },
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
            new SyntaxPair { Token = new TokenValue("base64data"), Name = "base64" },
            new SyntaxPair { Token = new TokenValue("}"), Name = "close-brace" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }
}
