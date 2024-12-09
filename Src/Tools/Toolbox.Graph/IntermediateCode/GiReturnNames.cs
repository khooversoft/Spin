using System.Collections.Immutable;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal sealed record GiReturnNames : ISelectInstruction
{
    public IReadOnlyList<string> ReturnNames { get; init; } = Array.Empty<string>();

    //public JournalEntry CreateJournal()
    //{
    //    var dataMap = new Dictionary<string, string?>
    //    {
    //        { GraphConstants.Trx.GiType, this.GetType().Name },
    //        { GraphConstants.Trx.GiData, this.ToJson() },
    //    };

    //    var journal = JournalEntry.Create(JournalType.Select, dataMap);
    //    return journal;
    //}

    public bool Equals(GiReturnNames? obj)
    {
        bool result = obj is GiReturnNames subject &&
            Enumerable.SequenceEqual(ReturnNames, subject.ReturnNames);

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(ReturnNames);
}

internal static class GiReturnNamesTool
{
    public static Option<ISelectInstruction> Build(InterContext interContext)
    {
        using var scope = interContext.NotNull().Cursor.IndexScope.PushWithScope();

        if (!interContext.Cursor.TryGetValue(out var selectValue) || selectValue.Token.Value != "return") return (StatusCode.NotFound, "no 'return' command found");

        var dataNames = new Sequence<string>();

        while (interContext.Cursor.TryGetValue(out var nextToken))
        {
            if (nextToken.MetaSyntaxName == "term")
            {
                interContext.Cursor.Index--;
                break;
            }

            if (nextToken.MetaSyntaxName == "comma") continue;

            if (nextToken.MetaSyntaxName != "dataName") return (StatusCode.BadRequest, "Expected data name");
            dataNames += nextToken.Token.Value;
        }

        scope.Cancel();
        return new GiReturnNames
        {
            ReturnNames = dataNames.ToImmutableArray()
        };
    }
}