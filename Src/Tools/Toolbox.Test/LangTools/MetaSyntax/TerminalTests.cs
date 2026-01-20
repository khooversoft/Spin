using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class TerminalTests
{
    private readonly IHost _host;

    public TerminalTests(ITestOutputHelper output)
    {
        _host = Host.CreateDefaultBuilder()
            .AddDebugLogging(x => output.WriteLine(x))
            .Build();
    }

    [Fact]
    public void TerminalSymbol()
    {
        string schemaText = new[]
        {
            "delimiters = ';' ;",
            "number = regex '^[+-]?[0-9]+$' ;",
            "term = ';' ;",
            "alias = number, term ;"
        }.Join(Environment.NewLine);

        var schema = MetaParser.ParseRules(schemaText);
        schema.StatusCode.IsOk().BeTrue(schema.Error);

        var parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, schema);

        var parse = parser.Parse("3;");
        parse.Status.IsOk().BeTrue();

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "alias",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("3"), Name = "number" },
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
            new SyntaxPair { Token = new TokenValue("3"), Name = "number" },
            new SyntaxPair { Token = new TokenValue(";"), Name = "term" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void TerminalSymbolRegexFail()
    {
        string schemaText = new[]
        {
            "delimiters = ';' ;",
            "number = regex '^[+-]?[0-9]+$' ;",
            "term = ';' ;",
            "alias = number, term ;"
        }.Join(Environment.NewLine);

        var schema = MetaParser.ParseRules(schemaText);
        schema.StatusCode.IsOk().BeTrue();

        var parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, schema);

        var parse = parser.Parse("A ;");
        parse.Status.IsError().BeTrue(parse.Status.Error);
        parse.Status.Error.Be("No rules matched");
    }
}
