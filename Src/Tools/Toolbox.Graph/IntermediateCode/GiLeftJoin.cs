using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal record GiLeftJoin : ISelectInstruction
{
    //public JournalEntry CreateJournal()
    //{
    //    var data = new KeyValuePair<string, string?>(GraphConstants.Trx.GiType, this.GetType().Name).ToEnumerable();
    //    var journal = JournalEntry.Create(JournalType.Select, data);
    //    return journal;
    //}
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