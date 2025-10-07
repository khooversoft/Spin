using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal record GiRightJoin : ISelectInstruction
{
}

internal static class GiRightJoinTool
{
    public static Option<ISelectInstruction> Build(InterContext interContext)
    {
        using var scope = interContext.NotNull().Cursor.IndexScope.PushWithScope();

        if (!interContext.Cursor.TryGetValue(out var leftJoin) || leftJoin.Name != "right-join") return (StatusCode.NotFound, "right-join");

        scope.Cancel();
        return new GiRightJoin();
    }

    public static string GetCommandDesc(this GiRightJoin subject)
    {
        var command = nameof(GiRightJoin);
        return command;
    }
}