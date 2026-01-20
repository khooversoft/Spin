using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;


namespace Toolbox.Graph.test.InterLang;

public class SelectNodesTests
{
    private readonly SyntaxParser _parser;

    public SelectNodesTests(ITestOutputHelper output)
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
    public void SelectAllNodes()
    {
        var parse = _parser.Parse("select (*) ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue(instructions.Error);

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiSelect
            {
                Instructions = [
                    new GiNodeSelect
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
    public void SelectNodeWithTag()
    {
        var parse = _parser.Parse("select (key=k1, t1) ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue(instructions.Error);

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiSelect
            {
                Instructions = [
                    new GiNodeSelect
                    {
                        Key = "k1",
                        Tags = new Dictionary<string, string?>
                        {
                            ["t1"] = null,
                        },
                    },
                ],
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void SelectNode()
    {
        var parse = _parser.Parse("select (key=k1) ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue(instructions.Error);

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiSelect
            {
                Instructions = [
                    new GiNodeSelect
                    {
                        Key = "k1",
                    },
                ],
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void SelectNodeAndTag()
    {
        var parse = _parser.Parse("select (key=user:k1, t1=first*) ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue(instructions.Error);

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiSelect
            {
                Instructions = [
                    new GiNodeSelect
                    {
                        Key = "user:k1",
                        Tags = new Dictionary<string, string?>
                        {
                            ["t1"] = "first*",
                        },
                    },
                ],
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void SelectNodeAndTag2AndData()
    {
        var parse = _parser.Parse("select (key=user:k1, t1=first*, t2) return data ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue(instructions.Error);

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiSelect
            {
                Instructions = [
                    new GiNodeSelect
                    {
                        Key = "user:k1",

                        Tags = new Dictionary<string, string?>
                        {
                            ["t1"] = "first*",
                            ["t2"] = null,
                        },
                    },
                    new GiReturnNames
                    {
                        ReturnNames = [ "data" ],
                    },
                ],
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }
}
