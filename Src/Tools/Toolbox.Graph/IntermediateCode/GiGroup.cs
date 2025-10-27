using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public enum GiGroupCommand
{
    None,
    Add,
    Delete,
    DeleteGroup
}

internal record GiGroup : IGraphInstruction
{
    public GiGroupCommand Command { get; init; }
    public string? PrincipalIdentifier { get; init; } = null!;
    public string GroupName { get; init; } = null!;
}

internal static class GiGroupTool
{
    public static Option<IGraphInstruction> Build(InterContext ic)
    {
        var userCommand = BuildAddUser(ic);
        if (userCommand.IsOk()) return userCommand;

        var deleteGroup = BuildDeleteGroupCommand(ic);
        if (deleteGroup.IsOk()) return deleteGroup;

        return StatusCode.NotFound;
    }

    public static Option<IGraphInstruction> BuildAddUser(InterContext ic)
    {
        using var scope = ic.NotNull().Cursor.IndexScope.PushWithScope();

        var command = ic.GetEnum<GiGroupCommand>("add-sym", "delete-sym");
        if (command.IsError()) return command.ToOptionStatus<IGraphInstruction>();
        if (!command.Return().Func(x => x == GiGroupCommand.Add || x == GiGroupCommand.Delete)) return StatusCode.NotFound;

        var principal = ic.GetValue("pi");
        if (principal.IsError()) return principal.ToOptionStatus<IGraphInstruction>();

        var s2 = ic.IsSymbol("to-sym", "from-sym");
        if (s2.IsError()) return s2.ToOptionStatus<IGraphInstruction>();

        var groupName = ic.GetValue("group-name");
        if (groupName.IsError()) return groupName.ToOptionStatus<IGraphInstruction>();

        var s4 = ic.IsSymbol("group-sym");
        if (s4.IsError()) return s4.ToOptionStatus<IGraphInstruction>();

        var term = ic.GetValue("term");
        if (term.IsError()) return term.ToOptionStatus<IGraphInstruction>();

        scope.Cancel();
        return new GiGroup()
        {
            Command = command.Return(),
            PrincipalIdentifier = principal.Return(),
            GroupName = groupName.Return(),
        };
    }

    private static Option<IGraphInstruction> BuildDeleteGroupCommand(InterContext ic)
    {
        using var scope = ic.NotNull().Cursor.IndexScope.PushWithScope();

        var command = ic.GetEnum<GiGroupCommand>("delete-sym");
        if (command.IsError()) return command.ToOptionStatus<IGraphInstruction>();
        if (command.Return() != GiGroupCommand.Delete) return StatusCode.NotFound;

        var groupName = ic.GetValue("group-name");
        if (groupName.IsError()) return groupName.ToOptionStatus<IGraphInstruction>();

        var s4 = ic.IsSymbol("group-sym");
        if (s4.IsError()) return s4.ToOptionStatus<IGraphInstruction>();

        var term = ic.GetValue("term");
        if (term.IsError()) return term.ToOptionStatus<IGraphInstruction>();

        scope.Cancel();
        return new GiGroup()
        {
            Command = GiGroupCommand.DeleteGroup,
            PrincipalIdentifier = null,
            GroupName = groupName.Return(),
        };
    }

    public static string GetCommandDesc(this GiGroup subject)
    {
        var command = new[]
        {
            nameof(GiGroup),
            $"Command={subject.Command}",
            $"PrincipalIdentifier={subject.PrincipalIdentifier ?? "null"}",
            $"GroupName={subject.GroupName}"
        }.Join(", ");

        return command;
    }
}
