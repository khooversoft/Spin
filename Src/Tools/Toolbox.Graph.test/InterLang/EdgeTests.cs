using FluentAssertions;
using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.InterLang;

public class EdgeTests : TestBase<EdgeTests>
{
    private readonly ITestOutputHelper _output;
    private readonly MetaSyntaxRoot _root;
    private readonly SyntaxParser _parser;
    private readonly ScopeContext _context;

    public EdgeTests(ITestOutputHelper output) : base(output)
    {
        _output = output.NotNull();

        string schema = GraphLanguageTool.ReadGraphLanguageRules();
        _root = MetaParser.ParseRules(schema);
        _root.StatusCode.IsOk().Should().BeTrue(_root.Error);

        _context = GetScopeContext();
        _parser = new SyntaxParser(_root);
    }

    [Theory]
    [InlineData("upsert edge from=fkey1, to=tkey1;")]
    [InlineData("upsert edge to=tkey1, type=label;")]
    [InlineData("upsert unique edge from=fkey1, to=tkey1, type=label;")]
    public void FailTest(string command)
    {
        var parse = _parser.Parse(command, _context);
        parse.Status.IsError().Should().BeTrue();
    }

    [Fact]
    public void MinAddEdge()
    {
        var parse = _parser.Parse("add edge from=fkey1, to=tkey1, type=label;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().Should().BeTrue(instructions.Error);

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiEdge
            {
                ChangeType = GiChangeType.Add,
                From = "fkey1",
                To = "tkey1",
                Type = "label",
                Unique = false,
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).Should().BeTrue();
    }

    [Fact]
    public void AddUniqueEdge()
    {
        var parse = _parser.Parse("add unique edge from=fkey1, to=tkey1, type=label;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().Should().BeTrue(instructions.Error);

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiEdge
            {
                ChangeType = GiChangeType.Add,
                From = "fkey1",
                To = "tkey1",
                Type = "label",
                Unique = true,
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).Should().BeTrue();
    }

    [Fact]
    public void AddEdgeWithTag()
    {
        var parse = _parser.Parse("add edge from=fkey1, to=tkey1, type=label set t1=v1;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().Should().BeTrue(instructions.Error);

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiEdge
            {
                ChangeType = GiChangeType.Add,
                From = "fkey1",
                To = "tkey1",
                Type = "label",
                Tags = new Dictionary<string, string?>
                {
                    ["t1"] = "v1",
                },
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).Should().BeTrue();
    }

    [Fact]
    public void AddUniqueEdgeWithTag()
    {
        var parse = _parser.Parse("add unique edge from=fkey1, to=tkey1, type=label set t1=v1;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().Should().BeTrue(instructions.Error);

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiEdge
            {
                ChangeType = GiChangeType.Add,
                From = "fkey1",
                To = "tkey1",
                Type = "label",
                Tags = new Dictionary<string, string?>
                {
                    ["t1"] = "v1",
                },
                Unique = true,
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).Should().BeTrue();
    }

    [Fact]
    public void UpsertNode()
    {
        var parse = _parser.Parse("upsert edge from=fkey1, to=tkey1, type=label;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().Should().BeTrue();

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiEdge
            {
                ChangeType = GiChangeType.Upsert,
                From = "fkey1",
                To = "tkey1",
                Type = "label",
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).Should().BeTrue();
    }

    [Fact]
    public void UpdateNodeTagCommand()
    {
        var parse = _parser.Parse("update edge from=fkey1, to=tkey1, type=label set t1;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().Should().BeTrue();

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiEdge
            {
                ChangeType = GiChangeType.Update,
                From = "fkey1",
                To = "tkey1",
                Type = "label",
                Tags = new Dictionary<string, string?>
                {
                    ["t1"] = null,
                },
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).Should().BeTrue();
    }

    [Fact]
    public void UpsertNodeRemoveTagCommand()
    {
        var parse = _parser.Parse("upsert edge from=fkey1, to=tkey1, type=label set -t1, t2=v2;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().Should().BeTrue();

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiEdge
            {
                ChangeType = GiChangeType.Upsert,
                From = "fkey1",
                To = "tkey1",
                Type = "label",
                Tags = new Dictionary<string, string?>
                {
                    ["-t1"] = null,
                    ["t2"] = "v2",
                },
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).Should().BeTrue();
    }
}
