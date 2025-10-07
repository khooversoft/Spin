using System.Collections.Frozen;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal sealed record GiEdgeSelect : ISelectInstruction
{
    public string? From { get; init; }
    public string? To { get; init; }
    public string? Type { get; init; }
    public IReadOnlyDictionary<string, string?> Tags { get; init; } = FrozenDictionary<string, string?>.Empty;
    public string? Alias { get; init; }

    //public JournalEntry CreateJournal() => [];

    public bool Equals(GiEdgeSelect? obj)
    {
        bool result = obj is GiEdgeSelect subject &&
            From == subject.From &&
            To == subject.To &&
            Type == subject.Type &&
            Tags.DeepEqualsComparer(subject.Tags) &&
            Alias == subject.Alias;

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(From, To, Type, Tags, Alias);
}

internal static class GiEdgeSelectTool
{
    public static Option<ISelectInstruction> Build(InterContext interContext)
    {
        using var scope = interContext.NotNull().Cursor.IndexScope.PushWithScope();

        string? from = null;
        string? to = null;
        string? edgeType = null;
        string? alias = null;
        var tags = new Dictionary<string, string?>();

        // [from=k1, to=k2, type=label, keys] a1
        if (!interContext.Cursor.TryGetValue(out var leftBracket) || leftBracket.Token.Value != "[") return (StatusCode.NotFound, "not found '['");

        var keyValuePairs = InterLangTool.GetAllKeyValues(interContext);
        foreach (var item in keyValuePairs)
        {
            switch (item)
            {
                case ("from", string value):
                    if (from.IsNotEmpty()) return (StatusCode.BadRequest, "Multiple 'from' specified");
                    from = value;
                    break;

                case ("to", string value):
                    if (to.IsNotEmpty()) return (StatusCode.BadRequest, "Multiple 'to' specified");
                    to = value;
                    break;

                case ("type", string value):
                    if (edgeType.IsNotEmpty()) return (StatusCode.BadRequest, "Multiple 'from' specified");
                    edgeType = value;
                    break;

                default:
                    tags.Add(item.Key, item.Value);
                    break;
            }
        }

        if (!interContext.Cursor.TryGetValue(out var rightBracket) || rightBracket.Token.Value != "]") return (StatusCode.NotFound, "not found ']'");

        if (interContext.Cursor.TryPeekValue(out var aliasValue) && aliasValue.Name == "alias")
        {
            interContext.Cursor.MoveNext();
            alias = aliasValue.Token.Value;
        }

        scope.Cancel();
        return new GiEdgeSelect
        {
            From = from,
            To = to,
            Type = edgeType,
            Tags = tags.ToFrozenDictionary(),
            Alias = alias,
        };
    }

    public static string GetCommandDesc(this GiEdgeSelect subject)
    {
        var command = nameof(GiEdgeSelect).ToEnumerable()
            .Append($"From={subject.From}")
            .Append($"To={subject.To}")
            .Append($"Type={subject.Type}")
            .Append($"Alias={subject.Alias}")
            .Append($"Tags={subject.Tags.ToTagsString()}")
            .Join(", ");

        return command;
    }
}
