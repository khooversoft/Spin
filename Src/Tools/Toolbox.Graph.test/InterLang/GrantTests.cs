using Toolbox.Extensions;
using Toolbox.Graph.test.Application;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.InterLang;

public class GrantTests : TestBase<NodeTests>
{
    private readonly ITestOutputHelper _output;
    private readonly MetaSyntaxRoot _root;
    private readonly SyntaxParser _parser;
    private readonly ScopeContext _context;

    public GrantTests(ITestOutputHelper output) : base(output)
    {
        _output = output.NotNull();

        string schema = GraphLanguageTool.ReadGraphLanguageRules();
        _root = MetaParser.ParseRules(schema);
        _root.StatusCode.IsOk().BeTrue(_root.Error);

        _context = GetScopeContext();
        _parser = new SyntaxParser(_root);
    }

    [Fact]
    public void SimpleGrant()
    {
        string cmd = "grant user1 reader on ticket:001 ;";

        var parse = _parser.Parse(cmd, _context);
        parse.Status.BeOk();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue();

        IGraphInstruction[] expected = [
            new GiGrant
            {
                GrantCommand = GrantCommand.Grant,
                GrantType = GrantType.Reader,
                PrincipalIdentifier = "user1",
                NameIdentifier = "ticket:001"
            },
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void SimpleRevoke()
    {
        string cmd = "revoke user1 reader on ticket:001 ;";

        var parse = _parser.Parse(cmd, _context);
        parse.Status.BeOk();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue();

        IGraphInstruction[] expected = [
            new GiGrant
            {
                GrantCommand = GrantCommand.Revoke,
                GrantType = GrantType.Reader,
                PrincipalIdentifier = "user1",
                NameIdentifier = "ticket:001"
            },
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }


    [Theory]
    [InlineData("grant reader to user1 user:user1;")]
    [InlineData("grant user1 on user:user1;")]
    [InlineData("grant badrole to user1 on user:user1;")]
    [InlineData("revoke badrole to user1 on user:user1;")]
    public void FailTest(string command)
    {
        var parse = _parser.Parse(command, _context);
        parse.Status.IsError().BeTrue();
    }

    [Theory]
    [InlineData("grant user1 reader on user:user1;", GrantCommand.Grant, GrantType.Reader, "user1", "user:user1")]
    [InlineData("grant user1 contributor on user:user1;", GrantCommand.Grant, GrantType.Contributor, "user1", "user:user1")]
    [InlineData("grant user1 owner on user:user1;", GrantCommand.Grant, GrantType.Owner, "user1", "user:user1")]
    [InlineData("revoke user1 reader on user:user1;", GrantCommand.Revoke, GrantType.Reader, "user1", "user:user1")]
    [InlineData("revoke user1 contributor on user:user1;", GrantCommand.Revoke, GrantType.Contributor, "user1", "user:user1")]
    [InlineData("revoke user1 owner on user:user1;", GrantCommand.Revoke, GrantType.Owner, "user1", "user:user1")]
    public void GrantPermission(string cmd, GrantCommand grantCmd, GrantType grantType, string user, string nameIdentifier)
    {
        var parse = _parser.Parse(cmd, _context);
        parse.Status.BeOk();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.IsOk().BeTrue();

        IGraphInstruction[] expected = [
            new GiGrant
            {
                GrantCommand = grantCmd,
                GrantType = grantType,
                PrincipalIdentifier = user,
                NameIdentifier = nameIdentifier
            },
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void SelectGrants()
    {
        string cmd = "select grants where role = reader ;";

        var parse = _parser.Parse(cmd, _context);
        parse.Status.BeOk();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.BeOk();
        IGraphInstruction[] expected = [
            new GiSelectObject
            {
                ObjectName = "grants",
                AttributeName = "role",
                Value = "reader"
            },
        ];
        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Theory]
    [InlineData("select grants where name = group ;", "grants", "name", "group")]
    [InlineData("select grants where role = reader ;", "grants", "role", "reader")]
    [InlineData("select grants where pi = user:user1 ;", "grants", "pi", "user:user1")]
    public void SelectGrant(string cmd, string objectName, string attributeName, string value)
    {
        var parse = _parser.Parse(cmd, _context);
        parse.Status.BeOk();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.BeOk();

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
