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
    [InlineData("delete user1 from admins group ;", GiGroupCommand.Delete, "user1", "admins")]
    [InlineData("delete admins group ;", GiGroupCommand.DeleteGroup, null, "admins")]
    [InlineData("add u-1 to eng-ops group ;", GiGroupCommand.Add, "u-1", "eng-ops")]
    [InlineData("add user1 to org/eng group ;", GiGroupCommand.Add, "user1", "org/eng")]
    [InlineData("add user1 to org:eng group ;", GiGroupCommand.Add, "user1", "org:eng")]
    [InlineData("delete user1 from org:eng group ;", GiGroupCommand.Delete, "user1", "org:eng")]
    public void AddDeleteUserFromGroup(string cmd, GiGroupCommand command, string? user, string group)
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
    [InlineData("select groups where name = admins ;", "groups", "name", "admins")]
    [InlineData("select groups where member = admins ;", "groups", "member", "admins")]
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

    // Additional coverage

    [Theory]
    [InlineData("add user1 admins group ;")]                // missing 'to'
    [InlineData("delete user1 admins group ;")]             // missing 'from'
    [InlineData("add user1 from admins group ;")]           // invalid: 'from' with add
    [InlineData("delete group admins ;")]                   // wrong order for delete-group
    [InlineData("delete admins group extra ;")]             // extra token before term
    [InlineData("delete group ;")]                          // missing group-name
    [InlineData("select groups name = admins ;")]           // missing 'where'
    [InlineData("select groups where name admins ;")]       // missing '='
    [InlineData("select groups where name = admins")]       // missing ';'
    public void TestBadSelectManagementPatterns(string cmd)
    {
        var parse = _parser.Parse(cmd, _context);
        parse.Status.BeError();
    }

    [Fact]
    public void MultiStatement_AddThenDeleteGroupMember()
    {
        string cmd = "add user1 to admins group ; delete user1 from admins group ;";

        var parse = _parser.Parse(cmd, _context);
        parse.Status.BeOk();

        var instructions = InterLangTool.Build(parse.SyntaxTree.GetAllSyntaxPairs().ToArray());
        instructions.BeOk();

        IGraphInstruction[] expected = [
            new GiGroup { Command = GiGroupCommand.Add, PrincipalIdentifier = "user1", GroupName = "admins" },
            new GiGroup { Command = GiGroupCommand.Delete, PrincipalIdentifier = "user1", GroupName = "admins" },
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void MultiStatement_AddThenSelectGroups()
    {
        string cmd = "add user1 to admins group ; select groups where member = user1 ;";

        var parse = _parser.Parse(cmd, _context);
        parse.Status.BeOk();

        var instructions = InterLangTool.Build(parse.SyntaxTree.GetAllSyntaxPairs().ToArray());
        instructions.BeOk();

        IGraphInstruction[] expected = [
            new GiGroup { Command = GiGroupCommand.Add, PrincipalIdentifier = "user1", GroupName = "admins" },
            new GiSelectObject { ObjectName = "groups", AttributeName = "member", Value = "user1" },
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }
}
