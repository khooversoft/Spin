using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.InterLang;

public class EdgeTests
{
    private readonly SyntaxParser _parser;

    public EdgeTests(ITestOutputHelper output)
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

    [Theory]
    [InlineData("upsert edge from=fkey1, to=tkey1;")]
    [InlineData("set edge from=fkey1, to=tkey1;")]
    [InlineData("set edge to=tkey1, type=label;")]
    [InlineData("delete edge from=fkey1, to=tkey1, type=label set t1;")]
    public void FailTest(string command)
    {
        var parse = _parser.Parse(command);
        parse.Status.IsError().BeTrue();
    }

    [Fact]
    public void MinAddEdge()
    {
        var parse = _parser.Parse("add edge from=fkey1, to=tkey1, type=label;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue(instructions.Error);

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

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void MinSetEdge()
    {
        var parse = _parser.Parse("set edge from=fkey1, to=tkey1, type=label;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue(instructions.Error);

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

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void MinDeleteEdge()
    {
        var parse = _parser.Parse("delete edge from=fkey1, to=tkey1, type=label;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue(instructions.Error);

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

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void AddEdgeWithTag()
    {
        var parse = _parser.Parse("add edge from=fkey1, to=tkey1, type=label set t1=v1;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue(instructions.Error);

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

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void SetEdge()
    {
        var parse = _parser.Parse("set edge from=fkey1, to=tkey1, type=label;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue();

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

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void SetEdge2()
    {
        var parse = _parser.Parse("set edge from=index:name1, to=data:key1, type=uniqueIndex;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue();

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

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void SetEdgeTagCommand()
    {
        var parse = _parser.Parse("set edge from=fkey1, to=tkey1, type=label set t1;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue();

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

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void SetEdgeRemoveTagCommand()
    {
        var parse = _parser.Parse("set edge from=fkey1, to=tkey1, type=label set -t1, t2=v2;");
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue();

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

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }
}
