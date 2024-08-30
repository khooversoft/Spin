using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal record GiLeftJoin : ISelectInstruction
{
}

internal static class GiLeftJoinTool
{
    public static Option<ISelectInstruction> Build(InterContext interContext)
    {
        using var scope = interContext.NotNull().Cursor.IndexScope.PushWithScope();

        if (!interContext.Cursor.TryGetValue(out var leftJoin) || leftJoin.MetaSyntaxName != "left-join") return (StatusCode.NotFound, "left-join");

        scope.Cancel();
        return new GiLeftJoin();
    }
}