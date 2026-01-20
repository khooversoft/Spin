using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class RepeatBaseRuleTests
{
    private readonly MetaSyntaxRoot _schema;
    private readonly SyntaxParser _parser;

    public RepeatBaseRuleTests(ITestOutputHelper output)
    {
        string schemaText = new[]
        {
            "delimiters          = , { } ;",
            "symbol              = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "comma               = ',' ;",
            "array               = symbol, { comma, symbol } ;",
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().BeTrue();

        var host = Host.CreateDefaultBuilder()
            .AddDebugLogging(x => output.WriteLine(x))
            .Build();

        _parser = ActivatorUtilities.CreateInstance<SyntaxParser>(host.Services, _schema);
    }

    [Fact]
    public void SimpleRepeat()
    {
        var parse = _parser.Parse("t1");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "array",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("t1"), Name = "symbol" },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("t1"), Name = "symbol" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void SimpleTwoRepeat()
    {
        var parse = _parser.Parse("t1, t2");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "array",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("t1"), Name = "symbol" },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_array-3-RepeatGroup",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
                                new SyntaxPair { Token = new TokenValue("t2"), Name = "symbol" },
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
            new SyntaxPair { Token = new TokenValue("t1"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("t2"), Name = "symbol" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void SimpleWithValueRepeat()
    {
        var parse = _parser.Parse("t1, v1, t2");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "array",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("t1"), Name = "symbol" },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_array-3-RepeatGroup",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
                                new SyntaxPair { Token = new TokenValue("v1"), Name = "symbol" },
                            },
                        },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_array-3-RepeatGroup",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
                                new SyntaxPair { Token = new TokenValue("t2"), Name = "symbol" },
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
            new SyntaxPair { Token = new TokenValue("t1"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("v1"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("t2"), Name = "symbol" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }
}
