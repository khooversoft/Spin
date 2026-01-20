using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class OptionalRuleOnly
{
    private readonly MetaSyntaxRoot _schema;
    private readonly IHost _host;

    public OptionalRuleOnly(ITestOutputHelper output)
    {
        string schemaText = new[]
        {
            "delimiters          = -> <-> ( ) ;",
            "join-left           = '->' ;",
            "join-inner          = '<->' ;",
            "join                = ( join-left | join-inner ) ;",
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().BeTrue();

        _host = Host.CreateDefaultBuilder()
            .AddDebugLogging(x => output.WriteLine(x))
            .Build();
    }

    [Theory]
    [InlineData("=")]
    [InlineData("<-")]
    [InlineData("raw")]
    public void FailMatches(string rawValue)
    {
        var parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);

        var parse = parser.Parse(rawValue);
        parse.Status.IsError().BeTrue(parse.Status.Error);
    }

    [Fact]
    public void LeftJoin()
    {
        var parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);

        var parse = parser.Parse("->");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
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
            },
        };

        (parse.SyntaxTree == expectedTree).BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = SyntaxTestTool.GenerateSyntaxPairs(syntaxPairs).Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("->"), Name = "join-left" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }

    [Fact]
    public void FullJoin()
    {
        var parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);

        var parse = parser.Parse("<->");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
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
                                new SyntaxPair { Token = new TokenValue("<->"), Name = "join-inner" },
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
            new SyntaxPair { Token = new TokenValue("<->"), Name = "join-inner" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }
}
