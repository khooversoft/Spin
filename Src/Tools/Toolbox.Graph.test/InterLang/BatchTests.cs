using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.InterLang;

public class BatchTests : TestBase<BatchTests>
{
    private readonly ITestOutputHelper _output;
    private readonly MetaSyntaxRoot _root;
    private readonly SyntaxParser _parser;
    private readonly ScopeContext _context;

    public BatchTests(ITestOutputHelper output) : base(output)
    {
        _output = output.NotNull();

        string schema = GraphLanguageTool.ReadGraphLanguageRules();
        _root = MetaParser.ParseRules(schema);
        _root.StatusCode.IsOk().Should().BeTrue(_root.Error);

        _context = GetScopeContext();
        _parser = new SyntaxParser(_root);
    }

    [Fact]
    public void MultipleCommandsNode()
    {
        string lines = new[]
{
            "add node key=k1;",
            "add node key=k2 set entity { 'aGVsbG9CYXNlNjQ=' };",
            "add edge from=k1, to=k2, type=user set k2=v2;",
            "select (userEmail) a1 -> [roles] a2 -> (*) a3 return data, entity;",
            "delete (userEmail);",
            "delete node key=k2;",
            "delete edge from=k1, to=k2, type=user;",
        }.Join(Environment.NewLine);

        var parse = _parser.Parse(lines, _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().Should().BeTrue();

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiNode
            {
                ChangeType = GiChangeType.Add,
                Key = "k1",
            },
            new GiNode
            {
                ChangeType = GiChangeType.Add,
                Key = "k2",
                Data = new Dictionary<string, string>
                {
                    ["entity"] = "aGVsbG9CYXNlNjQ=",
                },
            },
            new GiEdge
            {
                ChangeType = GiChangeType.Add,
                From = "k1",
                To = "k2",
                Type = "user",
                Tags = new Dictionary<string, string?>
                {
                    ["k2"] = "v2",
                },
            },
            new GiSelect
            {
                Instructions = [
                    new GiNodeSelect
                    {
                        Tags = new Dictionary<string, string?>
                        {
                            ["userEmail"] = null,
                        },
                        Alias = "a1",
                    },
                    new GiLeftJoin(),
                    new GiEdgeSelect
                    {
                        Tags = new Dictionary<string, string?>
                        {
                            ["roles"] = null,
                        },
                        Alias = "a2",
                    },
                    new GiLeftJoin(),
                    new GiNodeSelect
                    {
                        Tags = new Dictionary<string, string?>
                        {
                            ["*"] = null,
                        },
                        Alias = "a3",
                    },
                    new GiReturnNames
                    {
                        ReturnNames = [ "data", "entity" ],
                    },
                ],
            },
            new GiDelete
            {
                Instructions = [
                    new GiNodeSelect
                    {
                        Tags = new Dictionary<string, string?>
                        {
                            ["userEmail"] = null,
                        },
                    },
                ],
            },
            new GiNode
            {
                ChangeType = GiChangeType.Delete,
                Key = "k2",
            },
            new GiEdge
            {
                ChangeType = GiChangeType.Delete,
                From = "k1",
                To = "k2",
                Type = "user",
            },
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).Should().BeTrue();
    }
}
