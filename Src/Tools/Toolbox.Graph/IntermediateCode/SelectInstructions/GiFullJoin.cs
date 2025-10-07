using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal record GiFullJoin : ISelectInstruction
{
}

internal static class GiFullJoinTool
{
    public static Option<ISelectInstruction> Build(InterContext interContext)
    {
        using var scope = interContext.NotNull().Cursor.IndexScope.PushWithScope();

        if (!interContext.Cursor.TryGetValue(out var fullJoin) || fullJoin.Name != "full-join") return (StatusCode.NotFound, "full-join");

        scope.Cancel();
        return new GiFullJoin();
    }

    public static string GetCommandDesc(this GiFullJoin subject)
    {
        var command = nameof(GiFullJoin);
        return command;
    }
}