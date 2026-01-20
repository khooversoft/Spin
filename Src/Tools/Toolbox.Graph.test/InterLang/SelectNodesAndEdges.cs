using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.InterLang;

public class SelectNodesAndEdges
{
    private readonly SyntaxParser _parser;

    public SelectNodesAndEdges(ITestOutputHelper output)
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
    public void SelectAllNodesToEdgesLeftJoin()
    {
        var parse = _parser.Parse("select (*) -> [*] ;");
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
                    new GiLeftJoin(),
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
    public void SelectAllNodesToEdgesRightJoin()
    {
        var parse = _parser.Parse("select (*) <- [*] ;");
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
                    new GiRightJoin(),
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
    public void SelectAllNodesToEdgesToNodesLeftJoin()
    {
        var parse = _parser.Parse("select (*) -> [*] -> (*) ;");
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
                    new GiLeftJoin(),
                    new GiEdgeSelect
                    {
                        Tags = new Dictionary<string, string?>
                        {
                            ["*"] = null,
                        },
                    },
                    new GiLeftJoin(),
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
    public void SelectAllNodesToEdgesToNodesRightJoin()
    {
        var parse = _parser.Parse("select (*) <- [*] <- (*) ;");
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
                    new GiRightJoin(),
                    new GiEdgeSelect
                    {
                        Tags = new Dictionary<string, string?>
                        {
                            ["*"] = null,
                        },
                    },
                    new GiRightJoin(),
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
    public void SelectFullNodesToEdgesToNodes()
    {
        var parse = _parser.Parse("select (*) <-> [*] <-> (*) ;");
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
                    new GiFullJoin(),
                    new GiEdgeSelect
                    {
                        Tags = new Dictionary<string, string?>
                        {
                            ["*"] = null,
                        },
                    },
                    new GiFullJoin(),
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
    public void SelectFullAndRightNodesToEdgesToNodes()
    {
        var parse = _parser.Parse("select (*) <-> [*] <- (*) ;");
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
                    new GiFullJoin(),
                    new GiEdgeSelect
                    {
                        Tags = new Dictionary<string, string?>
                        {
                            ["*"] = null,
                        },
                    },
                    new GiRightJoin(),
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
    public void SelectAllIncorrectJoinsShouldFailAnalysis()
    {
        var parse = _parser.Parse("select (*) -> (*) -> (*) ;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsError().BeTrue(instructions.Error);
    }

    [Fact]
    public void SelectRelationship()
    {
        var parse = _parser.Parse("select (key=userEmail:user.com, indexer) <-> [userProfile] -> (*) return data, entity, player ;");
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
                        Key = "userEmail:user.com",
                        Tags = new Dictionary<string, string?>
                        {
                            ["indexer"] = null,
                        },
                    },
                    new GiFullJoin(),
                    new GiEdgeSelect
                    {
                        Tags = new Dictionary<string, string?>
                        {
                            ["userProfile"] = null,
                        },
                    },
                    new GiLeftJoin(),
                    new GiNodeSelect
                    {
                        Tags = new Dictionary<string, string?>
                        {
                            ["*"] = null,
                        },
                    },
                    new GiReturnNames
                    {
                        ReturnNames = [ "data", "entity", "player" ],
                    },
                ],
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }
}
