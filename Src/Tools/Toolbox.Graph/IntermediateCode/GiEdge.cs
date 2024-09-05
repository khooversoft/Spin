using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

internal sealed record GiEdge : IGraphInstruction
{
    public GiChangeType ChangeType { get; init; } = GiChangeType.None;
    public string From { get; init; } = null!;
    public string To { get; init; } = null!;
    public string Type { get; init; } = null!;
    public IReadOnlyDictionary<string, string?> Tags { get; init; } = ImmutableDictionary<string, string?>.Empty;

    public bool Equals(GiEdge? obj)
    {
        bool result = obj is GiEdge subject &&
            ChangeType == subject.ChangeType &&
            From == subject.From &&
            To == subject.To &&
            Type == subject.Type &&
            Tags.DeepEquals(subject.Tags);

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(ChangeType, From, To, Type, Tags);
}

internal static class GiEdgeTool
{
    public static Option<IGraphInstruction> Build(InterContext interContext)
    {
        using var scope = interContext.NotNull().Cursor.IndexScope.PushWithScope();

        // add edge from=fkey1, to=tkey1, type=label;
        if (!interContext.Cursor.TryGetValue(out var changeTypeSyntaxPair)) return (StatusCode.NotFound, "no add/upsert/update");
        if (!changeTypeSyntaxPair.Token.Value.TryToEnum<GiChangeType>(out var changeType, true)) return (StatusCode.BadRequest, "Invalid change type");

        // Edge
        if (!interContext.Cursor.TryGetValue(out var nodeValue) || nodeValue.Token.Value != "edge") return (StatusCode.NotFound, "no node");

        // from={key}
        if (!InterLangTool.TryGetValue(interContext, "from", out var fromNodeId)) return (StatusCode.NotFound, "Cannot find from=nodeKey");
        if (!interContext.Cursor.TryGetValue(out var comma1) || comma1.Token.Value != ",") return (StatusCode.NotFound, "comma ','");

        // to={key}
        if (!InterLangTool.TryGetValue(interContext, "to", out var toNodeId)) return (StatusCode.NotFound, "Cannot find to=nodeKey");
        if (!interContext.Cursor.TryGetValue(out var comma2) || comma2.Token.Value != ",") return (StatusCode.NotFound, "comma ','");

        // type={edgeType}
        if (!InterLangTool.TryGetValue(interContext, "type", out var edgeType)) return (StatusCode.NotFound, "Cannot find type={edgeType}");

        // Set
        Dictionary<string, string?>? tags = null;

        var tagsResult = InterLangTool.GetTags(interContext);
        if (tagsResult.IsOk())
        {
            tags = tagsResult.Value;
        }

        if (!interContext.Cursor.TryGetValue(out var term) || term.Token.Value != ";") return (StatusCode.NotFound, "term ';'");

        scope.Cancel();
        return new GiEdge
        {
            ChangeType = changeType,
            From = fromNodeId,
            To = toNodeId,
            Type = edgeType,
            Tags = tags?.ToImmutableDictionary() ?? ImmutableDictionary<string, string?>.Empty,
        };
    }
}
