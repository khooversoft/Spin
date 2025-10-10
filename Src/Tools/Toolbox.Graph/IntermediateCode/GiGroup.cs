using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public enum GiGroupCommand
{
    None,
    Add,
    Set,
    Delete,
}

internal record GiGroup : IGraphInstruction
{
    public GiGroupCommand Command { get; init; }
    public string PrincipalIdentifier { get; init; } = null!;
    public string GroupName { get; init; } = null!;
}

internal static class GiGroupTool
{
    /// example: grant {grantType} to {principal} on {nameIdentifier}
    /// grant types = Reader, Contributor, Owner
    public static Option<IGraphInstruction> Build(InterContext ic)
    {
        using var scope = ic.NotNull().Cursor.IndexScope.PushWithScope();

        var command = ic.GetEnum<GiGroupCommand>("add-sym", "set-sym", "delete-sym");
        if (command.IsError()) return command.ToOptionStatus<IGraphInstruction>();

        var principal = ic.GetValue("principalIdentifier");
        if (principal.IsError()) return principal.ToOptionStatus<IGraphInstruction>();

        var s3 = ic.IsSymbol("to-sym", "from-sym");
        if (s3.IsError()) return s3.ToOptionStatus<IGraphInstruction>();

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

    public static string GetCommandDesc(this GiGroup subject)
    {
        var command = new[]
        {
            nameof(GiGroup),
            $"Command={subject.Command}",
            $"PrincipalIdentifier={subject.PrincipalIdentifier}",
            $"GroupName={subject.GroupName}"
        }.Join(", ");

        return command;
    }
}
