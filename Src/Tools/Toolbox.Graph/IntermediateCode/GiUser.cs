using System.Collections.Frozen;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public enum UserCommand
{
    None,
    Add,
    Update,
    Delete
}

internal record GiUser : IGraphInstruction
{
    public UserCommand Command { get; init; }
    public string PrincipalId { get; init; } = null!;
    public string? NameIdentifier { get; init; }
    public string? UserName { get; init; }
    public string? Email { get; init; }
    public bool? EmailConfirmed { get; init; }
}

internal static class GiUserTool
{
    private static FrozenSet<string> _validTags = FrozenSet.Create<string>("ni", "name", "email", "emailConfirmed");

    public static Option<IGraphInstruction> Build(InterContext ic)
    {
        var updateUser = BuildUpdateUser(ic);
        if (updateUser.IsOk()) return updateUser;

        var deleteUser = BuildDeleteUser(ic);
        if (deleteUser.IsOk()) return deleteUser;

        return StatusCode.NotFound;
    }

    private static Option<IGraphInstruction> BuildUpdateUser(InterContext ic)
    {
        using var scope = ic.NotNull().Cursor.IndexScope.PushWithScope();

        var cmd = ic.GetEnum<UserCommand>("add-sym", "update-sym");
        if (cmd.IsError()) return cmd.ToOptionStatus<IGraphInstruction>();
        var command = cmd.Return();
        if (!(command == UserCommand.Add || command == UserCommand.Update)) return StatusCode.NotFound;

        var s1 = ic.ProcessSymbols(["user-sym"]);
        if (s1.IsError()) return s1.ToOptionStatus<IGraphInstruction>();

        var principal = ic.GetValue("pi");
        if (principal.IsError()) return principal.ToOptionStatus<IGraphInstruction>();

        var tagsOptions = InterLangTool.GetTags(ic);
        if (tagsOptions.IsError()) return tagsOptions.ToOptionStatus<IGraphInstruction>();
        var tags = tagsOptions.Return();
        if (tags.Count == 0) return (StatusCode.BadRequest, "User command must specify at least one tag");

        switch (command)
        {
            case UserCommand.Add:
                if (_validTags.SetEquals(tags.Keys) == false)
                {
                    return (StatusCode.BadRequest, "Add user must specify valid tags for user command");
                }

                foreach (var item in tags)
                {
                    if (item.Value.IsEmpty()) return (StatusCode.BadRequest, $"Add user tag '{item.Key}' must have a value");
                }

                break;

            case UserCommand.Update:
                if (!tags.Keys.All(x => _validTags.Contains(x)))
                {
                    return (StatusCode.BadRequest, "Update user contains invalid tags for user command");
                }
                break;
        }

        var term = ic.GetValue("term");
        if (term.IsError()) return term.ToOptionStatus<IGraphInstruction>();

        scope.Cancel();
        return new GiUser()
        {
            Command = cmd.Return(),
            PrincipalId = principal.Return(),
            NameIdentifier = tags.GetValueOrDefault("ni"),
            UserName = tags.GetValueOrDefault("name"),
            Email = tags.GetValueOrDefault("email"),
            EmailConfirmed = getConfirm(),
        };

        bool? getConfirm() => tags.TryGetValue("emailConfirmed", out var emailConfirmedStr) ?
            bool.TryParse(emailConfirmedStr, out var emailConfirmed) ? emailConfirmed : null : null;
    }

    private static Option<IGraphInstruction> BuildDeleteUser(InterContext ic)
    {
        using var scope = ic.NotNull().Cursor.IndexScope.PushWithScope();

        var cmd = ic.GetEnum<UserCommand>("delete-sym");
        if (cmd.IsError()) return cmd.ToOptionStatus<IGraphInstruction>();
        if (cmd.Return() != UserCommand.Delete) return StatusCode.NotFound;

        var s1 = ic.ProcessSymbols(["user-sym"]);
        if (s1.IsError()) return s1.ToOptionStatus<IGraphInstruction>();

        var principal = ic.GetValue("pi");
        if (principal.IsError()) return principal.ToOptionStatus<IGraphInstruction>();

        var term = ic.GetValue("term");
        if (term.IsError()) return term.ToOptionStatus<IGraphInstruction>();

        scope.Cancel();
        return new GiUser()
        {
            Command = cmd.Return(),
            PrincipalId = principal.Return(),
        };
    }

    public static string GetCommandDesc(this GiGrant subject)
    {
        var command = new[]
        {
            nameof(GiGrant),
            $"GrantType={subject.GrantType}",
            $"PrincipalIdentifier={subject.PrincipalIdentifier}",
            $"NameIdentifier={subject.NameIdentifier}"
        }.Join(", ");

        return command;
    }
}