using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;
using Xunit.Abstractions;

namespace Toolbox.Graph.test.InterLang;

public class UserTests
{
    private readonly SyntaxParser _parser;

    public UserTests(ITestOutputHelper output)
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

    [Fact]
    public void AddUser()
    {
        string cmd = "add user user1 set ni=v1, name=v2, email=v3, emailConfirmed=true ;";

        var parse = _parser.Parse(cmd);
        parse.Status.BeOk();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.BeOk();

        IGraphInstruction[] expected = [
            new GiUser
            {
                Command = UserCommand.Add,
                PrincipalId = "user1",
                NameIdentifier = "v1",
                UserName = "v2",
                Email = "v3",
                EmailConfirmed = true
            },
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void UpdateUser()
    {
        string cmd = "update user user1 set email=v3, emailConfirmed=true ;";

        var parse = _parser.Parse(cmd);
        parse.Status.BeOk();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.BeOk();

        IGraphInstruction[] expected = [
            new GiUser
            {
                Command = UserCommand.Update,
                PrincipalId = "user1",
                Email = "v3",
                EmailConfirmed = true
            },
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void DeleteUser()
    {
        string cmd = "delete user user1 ;";

        var parse = _parser.Parse(cmd);
        parse.Status.BeOk();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        instructions.BeOk();

        IGraphInstruction[] expected = [
            new GiUser
            {
                Command = UserCommand.Delete,
                PrincipalId = "user1",
            },
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();

        cmd = "delete user ;";
        parse = _parser.Parse(cmd);
        parse.Status.BeError();
    }

    [Theory]
    [InlineData("add user user1 set ni=v1, name=v2, email=v3, emailConfirmed=true ;", true)]
    [InlineData("add user user1 set ni=v1, name=v2, emailConfirmed=true ;", false)]
    [InlineData("add user user1 set ni=v1, email=v3, emailConfirmed=true ;", false)]
    [InlineData("add user user1 set name=v2, email=v3, emailConfirmed=true ;", false)]
    [InlineData("add user user1 set ni=v1, name=v2, email=v3 ;", false)]
    [InlineData("add user user1 set ni=v1, name=v2, email=v3, emailConfirmed=true, tooMany=v4 ;", false)]
    [InlineData("update user user1 set ni=v1, name=v2, email=v3, emailConfirmed=true ;", true)]
    [InlineData("update user user1 set ni=v1, name=v2, email=v3 ;", true)]
    [InlineData("update user user1 set ni=v1, name=v2, emailConfirmed=true ;", true)]
    [InlineData("update user user1 set ni=v1, email=v3, emailConfirmed=true ;", true)]
    [InlineData("update user user1 set name=v2, email=v3, emailConfirmed=true ;", true)]
    [InlineData("update user user1 set ni=v1, name=v2, email=v3, emailConfirmed=true, tooMany=v4 ;", false)]
    [InlineData("delete user user1 ;", true)]
    public void TestPatterns(string cmd, bool pass)
    {
        var parse = _parser.Parse(cmd);
        parse.Status.BeOk();

        var syntaxPairs = parse.SyntaxTree.GetAllSyntaxPairs().ToArray();
        var instructions = InterLangTool.Build(syntaxPairs);
        if (pass) instructions.BeOk(); else instructions.BeError();
    }

    // New tests for completeness and edge cases

    [Fact]
    public void AddUser_EmailConfirmedFalse()
    {
        string cmd = "add user u1 set ni=a, name=b, email=c, emailConfirmed=false ;";

        var parse = _parser.Parse(cmd);
        parse.Status.BeOk();

        var instructions = InterLangTool.Build(parse.SyntaxTree.GetAllSyntaxPairs().ToArray());
        instructions.BeOk();

        IGraphInstruction[] expected = [
            new GiUser
            {
                Command = UserCommand.Add,
                PrincipalId = "u1",
                NameIdentifier = "a",
                UserName = "b",
                Email = "c",
                EmailConfirmed = false
            },
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Theory]
    [InlineData("add user u1 set email=c, ni=a, name=b, emailConfirmed=true ;")]
    [InlineData("add user u1 set name=b, emailConfirmed=true, email=c, ni=a ;")]
    public void AddUser_OrderIrrelevant(string cmd)
    {
        var parse = _parser.Parse(cmd);
        parse.Status.BeOk();

        var instructions = InterLangTool.Build(parse.SyntaxTree.GetAllSyntaxPairs().ToArray());
        instructions.BeOk();

        instructions.Return().Count.Be(1);
        var gi = (GiUser)instructions.Return().Single();
        gi.Command.Be(UserCommand.Add);
        gi.PrincipalId.Be("u1");
        gi.NameIdentifier.Be("a");
        gi.UserName.Be("b");
        gi.Email.Be("c");
        gi.EmailConfirmed.BeTrue();
    }

    [Fact]
    public void AddUser_CaseSensitiveTagKeys_Fail()
    {
        // 'EmailConfirmed' wrong case should fail (keys are case-sensitive)
        string cmd = "add user u1 set ni=a, name=b, email=c, EmailConfirmed=true ;";

        var parse = _parser.Parse(cmd);
        parse.Status.BeOk();

        var instructions = InterLangTool.Build(parse.SyntaxTree.GetAllSyntaxPairs().ToArray());
        instructions.BeError();
    }

    [Fact]
    public void AddUser_EmailConfirmedCaseInsensitiveValue_Succeeds()
    {
        string cmd = "add user u1 set ni=a, name=b, email=c, emailConfirmed=TRUE ;";

        var parse = _parser.Parse(cmd);
        parse.Status.BeOk();

        var instructions = InterLangTool.Build(parse.SyntaxTree.GetAllSyntaxPairs().ToArray());
        instructions.BeOk();

        var gi = (GiUser)instructions.Return().Single();
        gi.EmailConfirmed.BeTrue();
    }

    [Fact]
    public void AddUser_InvalidEmailConfirmedValue_CurrentlyNull()
    {
        // Current behavior: invalid bool parses to null; add still succeeds because only keys are validated
        string cmd = "add user u1 set ni=a, name=b, email=c, emailConfirmed=notBool ;";

        var parse = _parser.Parse(cmd);
        parse.Status.BeOk();

        var instructions = InterLangTool.Build(parse.SyntaxTree.GetAllSyntaxPairs().ToArray());
        instructions.BeOk();

        var gi = (GiUser)instructions.Return().Single();
        gi.EmailConfirmed.BeNull();
    }

    [Fact]
    public void UpdateUser_NoTags_Fails()
    {
        string cmd = "update user u1 set ;";

        var parse = _parser.Parse(cmd);
        parse.Status.BeError();
    }

    [Fact]
    public void DeleteUser_WithTrailingTokens_Fails()
    {
        string cmd = "delete user u1 set x=y ;";

        var parse = _parser.Parse(cmd);
        parse.Status.BeError();
    }

    [Fact]
    public void MissingSemicolon_Fails()
    {
        string cmd = "add user u1 set ni=a, name=b, email=c, emailConfirmed=true";

        var parse = _parser.Parse(cmd);
        parse.Status.BeError();
    }

    [Fact]
    public void MultiStatement_AddThenUpdate()
    {
        string cmd =
            "add user u1 set ni=a, name=b, email=c, emailConfirmed=true ; " +
            "update user u1 set email=c2 ;";

        var parse = _parser.Parse(cmd);
        parse.Status.BeOk();

        var instructions = InterLangTool.Build(parse.SyntaxTree.GetAllSyntaxPairs().ToArray());
        instructions.BeOk();

        IGraphInstruction[] expected = [
            new GiUser
            {
                Command = UserCommand.Add,
                PrincipalId = "u1",
                NameIdentifier = "a",
                UserName = "b",
                Email = "c",
                EmailConfirmed = true
            },
            new GiUser
            {
                Command = UserCommand.Update,
                PrincipalId = "u1",
                Email = "c2",
            },
        ];

        Enumerable.SequenceEqual(instructions.Return(), expected).BeTrue();
    }

    [Fact]
    public void UpdateUser_DuplicateTags_Throws()
    {
        // Current implementation uses Dictionary.Add() and will throw on duplicate keys
        string cmd = "update user u1 set email=v1, email=v2 ;";

        var parse = _parser.Parse(cmd);
        parse.Status.BeOk();

        Action act = () => InterLangTool.Build(parse.SyntaxTree.GetAllSyntaxPairs().ToArray());
        Assert.Throws<ArgumentException>(act);
    }
}
