using System.Collections.Frozen;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.TransactionLog;
using Toolbox.Types;

namespace Toolbox.Graph;

internal sealed record GiNodeSelect : ISelectInstruction
{
    public string? Key { get; init; }
    public IReadOnlyDictionary<string, string?> Tags { get; init; } = FrozenDictionary<string, string?>.Empty;
    public string? Alias { get; init; }

    public JournalEntry CreateJournal()
    {
        var dataMap = new Dictionary<string, string?>
        {
            { GraphConstants.Trx.GiType, this.GetType().Name },
            { GraphConstants.Trx.GiData, this.ToJson() },
        };

        var journal = JournalEntry.Create(JournalType.Command, dataMap);
        return journal;
    }

    public bool Equals(GiNodeSelect? obj)
    {
        bool result = obj is GiNodeSelect subject &&
            Key == subject.Key &&
            Tags.DeepEquals(subject.Tags) &&
            Alias == subject.Alias;

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(Key, Tags, Alias);
}

internal static class GiNodeSelectTool
{
    public static Option<ISelectInstruction> Build(InterContext interContext)
    {
        using var scope = interContext.NotNull().Cursor.IndexScope.PushWithScope();

        string? key = null;
        string? alias = null;
        var tags = new Dictionary<string, string?>();

        // [key={key}, tags] a1
        if (!interContext.Cursor.TryGetValue(out var leftBracket) || leftBracket.Token.Value != "(") return (StatusCode.NotFound, "not found '('");

        var keyValuePairs = InterLangTool.GetAllKeyValues(interContext);
        foreach (var item in keyValuePairs)
        {
            switch (item)
            {
                case ("key", string value):
                    if (key.IsNotEmpty()) return (StatusCode.BadRequest, "Multiple 'from' specified");
                    key = value;
                    break;

                default:
                    tags.Add(item.Key, item.Value);
                    break;
            }
        }

        if (!interContext.Cursor.TryGetValue(out var rightBracket) || rightBracket.Token.Value != ")") return (StatusCode.NotFound, "not found ')'");

        if (interContext.Cursor.TryPeekValue(out var aliasValue) && aliasValue.MetaSyntaxName == "alias")
        {
            interContext.Cursor.MoveNext();
            alias = aliasValue.Token.Value;
        }

        scope.Cancel();
        return new GiNodeSelect
        {
            Key = key,
            Tags = tags.ToFrozenDictionary(),
            Alias = alias,
        };
    }
}
