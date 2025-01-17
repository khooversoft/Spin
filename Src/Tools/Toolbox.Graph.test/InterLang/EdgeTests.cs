using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Tools.Should;
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
    [InlineData("set edge from=fkey1, to=tkey1;")]
    [InlineData("set edge to=tkey1, type=label;")]
    [InlineData("delete edge from=fkey1, to=tkey1, type=label set t1;")]
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
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).Should().BeTrue();
    }

    [Fact]
    public void MinSetEdge()
    {
        var parse = _parser.Parse("set edge from=fkey1, to=tkey1, type=label;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().Should().BeTrue(instructions.Error);

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiEdge
            {
                ChangeType = GiChangeType.Set,
                From = "fkey1",
                To = "tkey1",
                Type = "label",
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).Should().BeTrue();
    }

    [Fact]
    public void MinDeleteEdge()
    {
        var parse = _parser.Parse("delete edge from=fkey1, to=tkey1, type=label;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().Should().BeTrue(instructions.Error);

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiEdge
            {
                ChangeType = GiChangeType.Delete,
                From = "fkey1",
                To = "tkey1",
                Type = "label",
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
    public void SetEdge()
    {
        var parse = _parser.Parse("set edge from=fkey1, to=tkey1, type=label;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().Should().BeTrue();

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiEdge
            {
                ChangeType = GiChangeType.Set,
                From = "fkey1",
                To = "tkey1",
                Type = "label",
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).Should().BeTrue();
    }

    [Fact]
    public void SetEdge2()
    {
        var parse = _parser.Parse("set edge from=index:name1, to=data:key1, type=uniqueIndex;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().Should().BeTrue();

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiEdge
            {
                ChangeType = GiChangeType.Set,
                From = "index:name1",
                To = "data:key1",
                Type = "uniqueIndex",
            },
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).Should().BeTrue();
    }

    [Fact]
    public void SetEdgeTagCommand()
    {
        var parse = _parser.Parse("set edge from=fkey1, to=tkey1, type=label set t1;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().Should().BeTrue();

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiEdge
            {
                ChangeType = GiChangeType.Set,
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
    public void SetEdgeRemoveTagCommand()
    {
        var parse = _parser.Parse("set edge from=fkey1, to=tkey1, type=label set -t1, t2=v2;", _context);
        parse.Status.IsOk().Should().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().Should().BeTrue();

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiEdge
            {
                ChangeType = GiChangeType.Set,
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
