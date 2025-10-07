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


    //[Theory]
    //[InlineData("add node;")]
    //[InlineData("delete node key=k1 set t1, entity { entityBase64 }, t2=v3, -t3, t5=v5, -data;")]
    //[InlineData("upsert node key=k1 set t1, t2=v2;")]
    //public void FailTest(string command)
    //{
    //    var parse = _parser.Parse(command, _context);
    //    parse.Status.IsError().BeTrue();
    //}

    [Theory]
    [InlineData("grant reader to user1 on user:user1;", GrantCommand.Grant, GrantType.Reader, "user1", "user:user1")]
    [InlineData("grant contributor to user1 on user:user1;", GrantCommand.Grant, GrantType.Contributor, "user1", "user:user1")]
    [InlineData("grant owner to user1 on user:user1;", GrantCommand.Grant, GrantType.Owner, "user1", "user:user1")]
    [InlineData("revoke reader to user1 on user:user1;", GrantCommand.Revoke, GrantType.Reader, "user1", "user:user1")]
    [InlineData("revoke contributor to user1 on user:user1;", GrantCommand.Revoke, GrantType.Contributor, "user1", "user:user1")]
    [InlineData("revoke owner to user1 on user:user1;", GrantCommand.Revoke, GrantType.Owner, "user1", "user:user1")]
    public void GrantPermission(string cmd, GrantCommand grantCmd, GrantType grantType, string user, string nameIdentifier)
    {
        var parse = _parser.Parse(cmd, _context);
        parse.Status.IsOk().BeTrue();

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

    [Theory]
    [InlineData("select grant where name = group ;", "grant", "name", "group")]
    [InlineData("select grant where role = reader ;", "grant", "name", "reader")]
    [InlineData("select grant where principal = user:user1 ;", "name", "grant", "user:user1")]
    public void SelectGrant(string cmd, string objectName, string attributeName, string value)
    {
        var parse = _parser.Parse(cmd, _context);
        parse.Status.IsOk().BeTrue();

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
