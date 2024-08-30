using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Toolbox.Extensions;
using Toolbox.LangTools;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public static class InterLangTool
{
    private static readonly Func<InterContext, Option<IGraphInstruction>>[] _call = [
        GiNodeTool.Build,
        GiEdgeTool.Build,
        GiSelectTool.Build,
    ];

    public static Option<IReadOnlyList<IGraphInstruction>> Build(IEnumerable<SyntaxPair> syntaxPairs)
    {
        var interContext = new InterContext(syntaxPairs);
        Sequence<IGraphInstruction> instructions = new Sequence<IGraphInstruction>();

        while (interContext.Cursor.TryPeekValue(out var nextToken))
        {
            if (nextToken.MetaSyntaxName == "term")
            {
                interContext.Cursor.MoveNext();
                continue;
            };

            var result = _call
                .Select(x => x(interContext))
                .SkipWhile(x => x.IsError())
                .Select(x => x.Return())
                .FirstOrDefaultOption(true);

            if (result.IsError()) return (StatusCode.BadRequest, "Failed to parse");
            instructions += result.Return();
        }

        return instructions.ToImmutableArray();
    }

    internal static Option<(string Key, string? Value)> GetKeyValue(InterContext interContext, bool valueRequired)
    {
        using var scope = interContext.NotNull().Cursor.IndexScope.PushWithScope();

        if (!interContext.Cursor.TryGetValue(out var keyValuePair)) return StatusCode.NotFound;

        if (interContext.Cursor.TryPeekValue(out var equalPair) && equalPair.Token.Value == "=")
        {
            interContext.Cursor.MoveNext();
            if (!interContext.Cursor.TryGetValue(out var valuePair)) return StatusCode.NotFound;

            scope.Cancel();
            return (keyValuePair.Token.Value, valuePair.Token.Value);
        }

        if (valueRequired) return StatusCode.NotFound;

        scope.Cancel();
        return (keyValuePair.Token.Value, null);
    }

    internal static Option<(string DataName, string Base64)> GetData(InterContext interContext)
    {
        using var scope = interContext.NotNull().Cursor.IndexScope.PushWithScope();

        if (!interContext.Cursor.TryGetValue(out var dataNamePair)) return StatusCode.NotFound;
        if (!interContext.Cursor.TryGetValue(out var openBrace) || openBrace.Token.Value != "{") return StatusCode.NotFound;
        if (!interContext.Cursor.TryGetValue(out var base64Pair)) return StatusCode.NotFound;
        if (!interContext.Cursor.TryGetValue(out var closeBrace) || closeBrace.Token.Value != "}") return StatusCode.NotFound;

        scope.Cancel();
        return (dataNamePair.Token.Value, base64Pair.Token.Value);
    }

    internal static IReadOnlyList<(string Key, string? Value)> GetAllKeyValues(InterContext interContext)
    {
        var keyValues = new List<(string Key, string? Value)>();

        while (interContext.Cursor.TryPeekValue(out var nextValue))
        {
            if (nextValue.MetaSyntaxName == "comma")
            {
                interContext.Cursor.MoveNext();
                continue;
            }

            if (nextValue.MetaSyntaxName != "tagKey" && nextValue.MetaSyntaxName != "key-value") break;

            var kv = GetKeyValue(interContext, false);
            if (kv.IsError()) break;

            keyValues.Add((kv.Value.Key, kv.Value.Value));
        }

        return keyValues.ToImmutableArray();
    }

    internal static Option<Dictionary<string, string?>> GetTags(InterContext interContext)
    {
        Dictionary<string, string?> tags = new Dictionary<string, string?>();

        using var scope = interContext.NotNull().Cursor.IndexScope.PushWithScope();

        if (!interContext.Cursor.TryGetValue(out var setValue) || setValue.Token.Value != "set") return (StatusCode.NotFound, "No 'set' or ';'");

        while (interContext.Cursor.TryPeekValue(out var nextValue))
        {
            if (nextValue.Token.Value == ";") break;

            if (nextValue.Token.Value == ",")
            {
                interContext.Cursor.MoveNext();
                continue;
            }

            var kv = GetKeyValue(interContext, false);
            if (kv.IsOk())
            {
                tags.Add(kv.Value.Key, kv.Value.Value);
                continue;
            }

            return (StatusCode.BadRequest, $"Invalid set command");
        }

        scope.Cancel();
        return tags;
    }

    internal static Option<(Dictionary<string, string?> Tags, Dictionary<string, string> Data)> GetTagsAndData(InterContext interContext)
    {
        Dictionary<string, string?> tags = new Dictionary<string, string?>();
        Dictionary<string, string> data = new Dictionary<string, string>();

        using var scope = interContext.NotNull().Cursor.IndexScope.PushWithScope();

        if (!interContext.Cursor.TryGetValue(out var setValue) || setValue.Token.Value != "set") return (StatusCode.NotFound, "No 'set' or ';'");

        while (interContext.Cursor.TryPeekValue(out var nextValue))
        {
            if (nextValue.Token.Value == ";") break;

            if (nextValue.Token.Value == ",")
            {
                interContext.Cursor.MoveNext();
                continue;
            }

            var dataOption = GetData(interContext);
            if (dataOption.IsOk())
            {
                data.Add(dataOption.Value.DataName, dataOption.Value.Base64);
                continue;
            }

            var kv = GetKeyValue(interContext, false);
            if (kv.IsOk())
            {
                tags.Add(kv.Value.Key, kv.Value.Value);
                continue;
            }

            return (StatusCode.BadRequest, $"Invalid set command");
        }

        scope.Cancel();
        return (tags, data);
    }

    internal static Option<string> GetValue(InterContext interContext, string keyValueToMatch)
    {
        var kvOption = InterLangTool.GetKeyValue(interContext, true);
        if (kvOption.IsError() || kvOption.Return().Func(x => x.Key != keyValueToMatch || x.Value.IsEmpty())) return (StatusCode.NotFound, "Cannot find key=nodeKey");

        return kvOption.Return().Value.NotNull();
    }

    internal static bool TryGetValue(InterContext interContext, string keyValueToMatch, [NotNullWhen(true)] out string? value)
    {
        var kvOption = InterLangTool.GetValue(interContext, keyValueToMatch);
        if (kvOption.IsError())
        {
            value = null;
            return false;
        }

        value = kvOption.Return().NotEmpty();
        return true;
    }
}
