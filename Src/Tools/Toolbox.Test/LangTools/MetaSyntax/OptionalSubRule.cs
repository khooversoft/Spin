using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class OptionalSubRule
{
    private readonly MetaSyntaxRoot _schema;
    private readonly SyntaxParser _parser;

    public OptionalSubRule(ITestOutputHelper output)
    {
        string schemaText = new[]
        {
            "delimiters          = -> <-> ( ) [ ] ;",
            "symbol              = regex '^[a-zA-Z\\*][a-zA-Z0-9\\-\\*]*$' ;",
            "join-left           = '->' ;",
            "join-inner          = '<->' ;",
            "join                = ( join-left | join-inner ) ;",
            "select              = symbol, [ join ] ;",
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().BeTrue(_schema.Error);

        var host = Host.CreateDefaultBuilder()
            .AddDebugLogging(x => output.WriteLine(x))
            .Build();

        _parser = ActivatorUtilities.CreateInstance<SyntaxParser>(host.Services, _schema);
    }

    [Fact]
    public void LeftJoin()
    {
        var parse = _parser.Parse("sym");
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
                        new SyntaxPair { Token = new TokenValue("sym"), Name = "symbol" },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("sym"), Name = "symbol" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }
}
