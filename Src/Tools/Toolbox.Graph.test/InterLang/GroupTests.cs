using Toolbox.Graph.test.Application;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.InterLang;

public class GroupTests : TestBase<NodeTests>
{
    private readonly ITestOutputHelper _output;
    private readonly MetaSyntaxRoot _root;
    private readonly SyntaxParser _parser;
    private readonly ScopeContext _context;

    public GroupTests(ITestOutputHelper output) : base(output)
    {
        _output = output.NotNull();

        string schema = GraphLanguageTool.ReadGraphLanguageRules();
        _root = MetaParser.ParseRules(schema);
        _root.StatusCode.IsOk().BeTrue(_root.Error);

        _context = GetScopeContext();
        _parser = new SyntaxParser(_root);
    }

    [Theory]
    [InlineData("add to admins group ;")]
    [InlineData("add user1 to group ;")]
    [InlineData("add user1 to admins")]
    [InlineData("select group badname = admins ;")]
    public void TestBadPatterns(string cmd)
    {
        var parse = _parser.Parse(cmd, _context);
        parse.Status.BeError();
    }

    [Theory]
    [InlineData("add user1 to admins group ;", GiGroupCommand.Add, "user1", "admins")]
    [InlineData("set user1 to admins group ;", GiGroupCommand.Set, "user1", "admins")]
    [InlineData("delete user1 from admins group ;", GiGroupCommand.Delete, "user1", "admins")]
    public void AddDeleteUserFromGroup(string cmd, GiGroupCommand command, string user, string group)
    {
        var parse = _parser.Parse(cmd, _context);
        parse.Status.BeOk();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue();

        IGraphInstruction[] expected = [
            new GiGroup
            {
                Command = command,
                PrincipalIdentifier = user,
                GroupName = group
            },
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Theory]
    [InlineData("select group where name = admins ;", "group", "name", "admins")]
    [InlineData("select group where member = admins ;", "group", "member", "admins")]
    public void Select(string cmd, string objectName, string attributeName, string value)
    {
        var parse = _parser.Parse(cmd, _context);
        parse.Status.BeOk();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue();

        IGraphInstruction[] expected = [
            new GiSelectObject
            {
                ObjectName = objectName,
                AttributeName = attributeName,
                Value = value
            },
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }
}
