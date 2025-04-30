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
        _root.StatusCode.IsOk().BeTrue(_root.Error);

        _context = GetScopeContext();
        _parser = new SyntaxParser(_root);
    }


    [Theory]
    [InlineData("add node;")]
    [InlineData("delete node key=k1 set t1, entity { entityBase64 }, t2=v3, -t3, t5=v5, -data;")]
    [InlineData("upsert node key=k1 set t1, t2=v2;")]
    public void FailTest(string command)
    {
        var parse = _parser.Parse(command, _context);
        parse.Status.IsError().BeTrue();
    }

    [Fact]
    public void MinAddNode()
    {
        var parse = _parser.Parse("add node key=k1;", _context);
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue();

        IGraphInstruction[] expected = [
            new GiNode { ChangeType = GiChangeType.Add, Key="k1" },
            ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void MinDeleteNode()
    {
        var parse = _parser.Parse("delete node key=k1;", _context);
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue();

        IGraphInstruction[] expected = [
            new GiNode { ChangeType = GiChangeType.Delete, Key="k1" },
            ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void SetNodeWithTags()
    {
        var parse = _parser.Parse("set node key=k1 set t1, t2=v2;", _context);
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue();

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiNode
            {
                ChangeType = GiChangeType.Set,
                Key = "k1",
                Tags = new Dictionary<string, string?>
                {
                    ["t2"] = "v2",
                    ["t1"] = null,
                },
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void SetNodeRemoveTagCommand()
    {
        var parse = _parser.Parse("set node key=k1 set -t1, t2=v2;", _context);
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue();

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiNode
            {
                ChangeType = GiChangeType.Set,
                Key = "k1",
                Tags = new Dictionary<string, string?>
                {
                    ["t2"] = "v2",
                    ["-t1"] = null,
                },
            }
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void AddNodeCommand()
    {
        var parse = _parser.Parse("add node key=k1 set t1, entity { 'aGVsbG8=' }, t2=v3, t3, data { 'aGVsbG8y' };", _context);
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue();

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

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void SetNodeCommand()
    {
        var parse = _parser.Parse("set node key=k1 set t1, entity { entityBase64 }, t2=v3, -t3, t5=v5, -data;", _context);
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue();

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiNode
            {
                ChangeType = GiChangeType.Set,
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

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void SetCommand2()
    {
        var parse = _parser.Parse("set node key=data:key1 set entity { 'eyJrZXkiOiJrZXkxIiwibmFtZSI6bnVsbCwiYWdlIjpudWxsLCJwcm92aWRlciI6bnVsbCwicHJvdmlkZXJLZXkiOm51bGx9' };", _context);
        parse.Status.IsOk().BeTrue(parse.ToString());

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue();

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiNode
            {
                ChangeType = GiChangeType.Set,
                Key = "data:key1",
                Data = new Dictionary<string, string>
                {
                    ["entity"] = "eyJrZXkiOiJrZXkxIiwibmFtZSI6bnVsbCwiYWdlIjpudWxsLCJwcm92aWRlciI6bnVsbCwicHJvdmlkZXJLZXkiOm51bGx9",
                },
            },
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void SetIndex()
    {
        var cmd = "set node key=user:username1@company.com set loginProvider=userEmail:username1@domain1.com, email=userEmail:username1@domain1.com index loginProvider ;";
        var parse = _parser.Parse(cmd, _context);
        parse.Status.IsOk().BeTrue(parse.Status.ToString());

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue(instructions.ToString());

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiNode
            {
                ChangeType = GiChangeType.Set,
                Key = "user:username1@company.com",
                Tags = new Dictionary<string, string?>
                {
                    ["email"] = "userEmail:username1@domain1.com",
                    ["loginProvider"] = "userEmail:username1@domain1.com",
                },
                Indexes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "loginProvider",
                },
            },
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void RemoveIndex()
    {
        var cmd = "set node key=user:username1@company.com set loginProvider=userEmail:username1@domain1.com, email=userEmail:username1@domain1.com index -loginProvider ;";
        var parse = _parser.Parse(cmd, _context);
        parse.Status.IsOk().BeTrue(parse.Status.ToString());

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue(instructions.ToString());

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiNode
            {
                ChangeType = GiChangeType.Set,
                Key = "user:username1@company.com",
                Tags = new Dictionary<string, string?>
                {
                    ["email"] = "userEmail:username1@domain1.com",
                    ["loginProvider"] = "userEmail:username1@domain1.com",
                },
                Indexes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "-loginProvider",
                },
            },
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void SetIndex2()
    {
        var cmd = "set node key=user:username1@company.com set loginProvider=userEmail:username1@domain1.com, email=userEmail:username1@domain1.com index loginProvider, email ;";
        var parse = _parser.Parse(cmd, _context);
        parse.Status.IsOk().BeTrue(parse.Status.ToString());

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue(instructions.ToString());

        string expectedList = GraphTestTool.GenerateTestCodeSyntaxTree(instructions.Return()).Join(Environment.NewLine);

        IGraphInstruction[] expected = [
            new GiNode
            {
                ChangeType = GiChangeType.Set,
                Key = "user:username1@company.com",
                Tags = new Dictionary<string, string?>
                {
                    ["email"] = "userEmail:username1@domain1.com",
                    ["loginProvider"] = "userEmail:username1@domain1.com",
                },
                Indexes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "email",
                    "loginProvider",
                },
            },
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void AddNodeFK()
    {
        var parse = _parser.Parse("add node key=k1 set t1=v1 foreignkey t1 ;", _context);
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructionsOption = InterLangTool.Build(syntaxPairs);
        instructionsOption.IsOk().BeTrue();

        IGraphInstruction[] expected = [
            new GiNode
            {
                ChangeType = GiChangeType.Add,
                Key="k1",
                Tags = new Dictionary<string, string?>
                {
                    ["t1"] = "v1",
                },
                ForeignKeys = "t1".ToTags()
            }];

        var instructions = instructionsOption.Return();
        Enumerable.SequenceEqual(instructions, expected).BeTrue();
    }

    [Fact]
    public void AddNodeFKTwo()
    {
        var parse = _parser.Parse("add node key=k1 set t1=v1, t2=v2 foreignkey t1, t2=p1* ;", _context);
        parse.Status.IsOk().BeTrue();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructionsOption = InterLangTool.Build(syntaxPairs);
        instructionsOption.IsOk().BeTrue();

        IGraphInstruction[] expected = [
            new GiNode
            {
                ChangeType = GiChangeType.Add,
                Key="k1",
                Tags = new Dictionary<string, string?>
                {
                    ["t1"] = "v1",
                    ["t2"] = "v2",
                },
                ForeignKeys = "t1,t2=p1*".ToTags()
            }];

        var instructions = instructionsOption.Return();
        Enumerable.SequenceEqual(instructions, expected).BeTrue();
    }
}
