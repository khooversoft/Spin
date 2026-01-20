using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Test.LangTools.MetaSyntax.GraphModel;

public class ReturnDataTests
{
    private readonly MetaSyntaxRoot _schema;
    private readonly IHost _host;

    public ReturnDataTests(ITestOutputHelper output)
    {
        string schemaText = new[]
        {
            "delimiters          = , { } ;",
            "symbol              = regex '^[a-zA-Z][a-zA-Z0-9\\-]*$' ;",
            "comma               = ',' ;",
            "return-sym          = 'return' ;",
            "return-query        = return-sym, symbol, { comma, symbol } ;",
        }.Join(Environment.NewLine);

        string schema = GraphModelTool.ReadGraphLanauge2();

        _schema = MetaParser.ParseRules(schemaText);
        _schema.StatusCode.IsOk().BeTrue();

        _host = Host.CreateDefaultBuilder()
            .AddDebugLogging(x => output.WriteLine(x))
            .Build();
    }

    [Theory]
    [InlineData("return d1 f1")]
    [InlineData("return")]
    public void FailedReturn(string command)
    {
        var parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);

        var parse = parser.Parse(command);
        parse.Status.IsError().BeTrue(parse.Status.Error);
    }

    [Fact]
    public void SimpleLabelAndRepeat()
    {
        var parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);

        var parse = parser.Parse("return d1");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = parse.SyntaxTree.GenerateTestCodeSyntaxTree().Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "return-query",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("return"), Name = "return-sym" },
                        new SyntaxPair { Token = new TokenValue("d1"), Name = "symbol" },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("return"), Name = "return-sym" },
            new SyntaxPair { Token = new TokenValue("d1"), Name = "symbol" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }


    [Fact]
    public void MultipleData()
    {
        var parser = ActivatorUtilities.CreateInstance<SyntaxParser>(_host.Services, _schema);

        var parse = parser.Parse("return d1, d2");
        parse.Status.IsOk().BeTrue(parse.Status.Error);

        var lines = parse.SyntaxTree.GenerateTestCodeSyntaxTree().Join(Environment.NewLine);

        var expectedTree = new SyntaxTree
        {
            Children = new ISyntaxTree[]
            {
                new SyntaxTree
                {
                    MetaSyntaxName = "return-query",
                    Children = new ISyntaxTree[]
                    {
                        new SyntaxPair { Token = new TokenValue("return"), Name = "return-sym" },
                        new SyntaxPair { Token = new TokenValue("d1"), Name = "symbol" },
                        new SyntaxTree
                        {
                            MetaSyntaxName = "_return-query-5-RepeatGroup",
                            Children = new ISyntaxTree[]
                            {
                                new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
                                new SyntaxPair { Token = new TokenValue("d2"), Name = "symbol" },
                            },
                        },
                    },
                },
            },
        };

        (parse.SyntaxTree == expectedTree).BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var syntaxLines = syntaxPairs.GenerateSyntaxPairs().Join(Environment.NewLine);

        var expectedPairs = new[]
        {
            new SyntaxPair { Token = new TokenValue("return"), Name = "return-sym" },
            new SyntaxPair { Token = new TokenValue("d1"), Name = "symbol" },
            new SyntaxPair { Token = new TokenValue(","), Name = "comma" },
            new SyntaxPair { Token = new TokenValue("d2"), Name = "symbol" },
        };

        syntaxPairs.SequenceEqual(expectedPairs).BeTrue();
    }

}
