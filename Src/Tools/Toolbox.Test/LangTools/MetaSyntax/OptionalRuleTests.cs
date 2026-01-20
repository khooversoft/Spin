using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax;

public class OptionalRuleTests
{
    private readonly MetaSyntaxRoot _schema;
    private readonly IHost _host;
    private readonly ITestOutputHelper _output;

    public OptionalRuleTests(ITestOutputHelper output)
    {
        _output = output;

        string schemaText = new[]
        {
            "delimiters          = [ ] = ;",
            "symbol              = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "tag                 = symbol, [ '=', symbol ] ;",
        }.Join(Environment.NewLine);

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().BeTrue();

        _host = Host.CreateDefaultBuilder()
            .AddDebugLogging(x => output.WriteLine(x))
            .Build();
    }

    [Fact]
    public void OnlyRequiredOfOptional()
    {
        var parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);

        var parse = parser.Parse("t1");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
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
    public void OnlyRequiredAndOptional()
    {
        var parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);

        var parse = parser.Parse("t1 = v1");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = SyntaxTestTool.GenerateTestCodeSyntaxTree(parse.SyntaxTree).Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
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
                                new SyntaxPair { Token = new TokenValue("v1"), Name = "symbol" },
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
            new SyntaxPair { Token = new TokenValue("="), Name = "_tag-3-OptionGroup-1" },
            new SyntaxPair { Token = new TokenValue("v1"), Name = "symbol" },
        };

        Enumerable.SequenceEqual(syntaxPairs, expectedPairs).BeTrue();
    }
}
