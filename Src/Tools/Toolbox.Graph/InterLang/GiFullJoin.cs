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

        if (!interContext.Cursor.TryGetValue(out var fullJoin) || fullJoin.MetaSyntaxName != "full-join") return (StatusCode.NotFound, "full-join");

        scope.Cancel();
        return new GiFullJoin();
    }
}