using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.InterLang;

public class DeleteNodeAndEdges : TestBase<DeleteNodeAndEdges>
{
    private readonly ITestOutputHelper _output;
    private readonly MetaSyntaxRoot _root;
    private readonly SyntaxParser _parser;
    private readonly ScopeContext _context;

    public DeleteNodeAndEdges(ITestOutputHelper output) : base(output)
    {
        _output = output.NotNull();

        string schema = GraphLanguageTool.ReadGraphLanguageRules();
        _root = MetaParser.ParseRules(schema);
        _root.StatusCode.IsOk().BeTrue(_root.Error);

        _context = GetScopeContext();
        _parser = new SyntaxParser(_root);
    }


    [Fact]
    public void DeleteAllNodes()
    {
        var parse = _parser.Parse("delete (*) ;", _context);
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue(instructions.Error);

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiDelete
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
    public void DeleteAllEdges()
    {
        var parse = _parser.Parse("delete [*] ;", _context);
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue(instructions.Error);

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiDelete
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
    public void DeleteEdgesWithTag()
    {
        var parse = _parser.Parse("delete [user] ;", _context);
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue(instructions.Error);

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiDelete
            {
                Instructions = [
                    new GiEdgeSelect
                    {
                        Tags = new Dictionary<string, string?>
                        {
                            ["user"] = null,
                        },
                    },
                ],
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void DeleteUserEdges()
    {
        var parse = _parser.Parse("delete (user) -> [*] ;", _context);
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue(instructions.Error);

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiDelete
            {
                Instructions = [
                    new GiNodeSelect
                    {
                        Tags = new Dictionary<string, string?>
                        {
                            ["user"] = null,
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
    public void DeleteAllNodesToEdgesToNodes()
    {
        var parse = _parser.Parse("delete (*) -> [*] -> (*) ;", _context);
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue(instructions.Error);

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiDelete
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
    public void deleteFullNodesToEdgesToNodes()
    {
        var parse = _parser.Parse("delete (*) <-> [*] <-> (*) ;", _context);
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue(instructions.Error);

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiDelete
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
    public void DeleteAllIncorrectJoinsShouldFailAnalysis()
    {
        var parse = _parser.Parse("delete (*) -> (*) -> (*) ;", _context);
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsError().BeTrue(instructions.Error);
    }

    [Fact]
    public void SelectRelationship()
    {
        var parse = _parser.Parse("select (key=userEmail:user.com, indexer) <-> [userProfile] -> (*) return data, entity, player ;", _context);
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
