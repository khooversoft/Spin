using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.InterLang;

public class NodeTests : TestBase<NodeTests>
{
    private readonly ITestOutputHelper _output;
    private readonly MetaSyntaxRoot _root;
    private readonly SyntaxParser _parser;
    private readonly ScopeContext _context;

    public NodeTests(ITestOutputHelper output) : base(output)
    {
        _output = output.NotNull();

        string schema = GraphLanguageTool.ReadGraphLanguageRules();
        _root = MetaParser.ParseRules(schema);
        _root.StatusCode.IsOk().Should().BeTrue(_root.Error);

        _context = GetScopeContext();
        _parser = new SyntaxParser(_root);
    }

    [Fact]
    public void MinAddNode()
    {
        var parse = _parser.Parse("add node key=k1;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().Should().BeTrue();

        IGraphInstruction[] expected = [
            new GiNode { ChangeType = GiChangeType.Add, Key="k1" },
            ];

        Enumerable.SequenceEqual(instructions.Return(), expected).Should().BeTrue();
    }

    [Fact]
    public void UpsertNode()
    {
        var parse = _parser.Parse("upsert node key=k1 set t1, t2=v2;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().Should().BeTrue();

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiNode
            {
                ChangeType = GiChangeType.Upsert,
                Key = "k1",
                Tags = new Dictionary<string, string?>
                {
                    ["t2"] = "v2",
                    ["t1"] = null,
                },
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).Should().BeTrue();
    }

    [Fact]
    public void UpsertNodeRemoveTagCommand()
    {
        var parse = _parser.Parse("upsert node key=k1 set -t1, t2=v2;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().Should().BeTrue();

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiNode
            {
                ChangeType = GiChangeType.Upsert,
                Key = "k1",
                Tags = new Dictionary<string, string?>
                {
                    ["t2"] = "v2",
                    ["-t1"] = null,
                },
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).Should().BeTrue();
    }

    [Fact]
    public void AddNodeCommand()
    {
        var parse = _parser.Parse("add node key=k1 set t1, entity { 'aGVsbG8=' }, t2=v3, t3, data { 'aGVsbG8y' };", _context);
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
                Tags = new Dictionary<string, string?>
                {
                    ["t2"] = "v3",
                    ["t3"] = null,
                    ["t1"] = null,
                },
                Data = new Dictionary<string, string>
                {
                    ["data"] = "aGVsbG8y",
                    ["entity"] = "aGVsbG8=",
                },
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).Should().BeTrue();
    }

    [Fact]
    public void UpdateNodeCommand()
    {
        var parse = _parser.Parse("update node key=k1 set t1, entity { entityBase64 }, t2=v3, -t3, t5=v5, -data;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().Should().BeTrue();

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiNode
            {
                ChangeType = GiChangeType.Update,
                Key = "k1",
                Tags = new Dictionary<string, string?>
                {
                    ["-t3"] = null,
                    ["t2"] = "v3",
                    ["t5"] = "v5",
                    ["-data"] = null,
                    ["t1"] = null,
                },
                Data = new Dictionary<string, string>
                {
                    ["entity"] = "entityBase64",
                },
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).Should().BeTrue();
    }
}
