using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.InterLang;

public class SelectEdgeTests
{
    private readonly SyntaxParser _parser;

    public SelectEdgeTests(ITestOutputHelper output)
    {
        var host = Host.CreateDefaultBuilder()
            .AddDebugLogging(x => output.WriteLine(x))
            .ConfigureServices((context, services) =>
            {
                services.AddInMemoryKeyStore();
                services.AddGraphEngine(config => config.BasePath = "basePath");
            })
            .Build();

        _parser = ActivatorUtilities.CreateInstance<SyntaxParser>(host.Services);
    }

    [Fact]
    public void SelectAllEdges()
    {
        var parse = _parser.Parse("select [*] ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue(instructions.Error);

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiSelect
            {
                Instructions = [
                    new GiEdgeSelect
                    {
                        Tags = new Dictionary<string, string?>
                        {
                            ["*"] = null,
                        },
                    },
                ],
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void SelectEdge()
    {
        var parse = _parser.Parse("select [from=user:f1, to=t1, type=user] ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue(instructions.Error);

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiSelect
            {
                Instructions = [
                    new GiEdgeSelect
                    {
                        From = "user:f1",
                        To = "t1",
                        Type = "user",
                    },
                ],
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void SelectEdgeWithTag()
    {
        var parse = _parser.Parse("select [from=user:f1, to=t1, type=user, t1=contact*] ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue(instructions.Error);

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiSelect
            {
                Instructions = [
                    new GiEdgeSelect
                    {
                        From = "user:f1",
                        To = "t1",
                        Type = "user",
                        Tags = new Dictionary<string, string?>
                        {
                            ["t1"] = "contact*",
                        },
                    },
                ],
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }


    [Fact]
    public void SelectEdgeWithData()
    {
        var parse = _parser.Parse("select [from=user:f1, to=t1, type=user, t1=contact*] return entity, data ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue(instructions.Error);

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiSelect
            {
                Instructions = [
                    new GiEdgeSelect
                    {
                        From = "user:f1",
                        To = "t1",
                        Type = "user",
                        Tags = new Dictionary<string, string?>
                        {
                            ["t1"] = "contact*",
                        },
                    },
                    new GiReturnNames
                    {
                        ReturnNames = [ "entity", "data" ],
                    },
                ],
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }
}
