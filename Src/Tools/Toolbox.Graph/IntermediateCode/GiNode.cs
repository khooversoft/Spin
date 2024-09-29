﻿using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.TransactionLog;
using Toolbox.Types;

namespace Toolbox.Graph;


public sealed record GiNode : IGraphInstruction
{
    public GiChangeType ChangeType { get; init; } = GiChangeType.None;
    public string Key { get; init; } = null!;
    public IReadOnlyDictionary<string, string?> Tags { get; init; } = ImmutableDictionary<string, string?>.Empty;
    public IReadOnlyDictionary<string, string> Data { get; init; } = ImmutableDictionary<string, string>.Empty;
    public bool IfExist { get; init; }

    public IReadOnlyList<JournalEntry> CreateJournals()
    {
        var dataMap = new Dictionary<string, string?>
        {
            { GraphConstants.Trx.GiChangeType, this.GetType().Name },
            { GraphConstants.Trx.GiData, this.ToJson() },
        };

        ImmutableArray<JournalEntry> journal = [JournalEntry.Create(JournalType.Command, dataMap)];
        return journal;
    }

    public bool Equals(GiNode? obj)
    {
        bool result = obj is GiNode subject &&
            ChangeType == subject.ChangeType &&
            Key == subject.Key &&
            Tags.DeepEquals(subject.Tags) &&
            Enumerable.SequenceEqual(Data.OrderBy(x => x.Key), subject.Data.OrderBy(x => x.Key)) &&
            IfExist == subject.IfExist;

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(ChangeType, Key, Tags, Data);
    public override string ToString() => $"ChangeType={ChangeType}, Key={Key}, Tags={Tags.ToTagsString()}, Data={Data.Select(x => x.Key).Join(";")}";
}

internal static class GiNodeTool
{
    public static Option<IGraphInstruction> Build(InterContext interContext)
    {
        using var scope = interContext.NotNull().Cursor.IndexScope.PushWithScope();

        // add node key={key}
        if (!interContext.Cursor.TryGetValue(out var changeTypeSyntaxPair)) return (StatusCode.NotFound, "no add/upsert/update");
        if (!changeTypeSyntaxPair.Token.Value.TryToEnum<GiChangeType>(out var changeType, true)) return (StatusCode.BadRequest, "Invalid change type");

        // node
        if (!interContext.Cursor.TryGetValue(out var nodeValue) || nodeValue.Token.Value != "node") return (StatusCode.NotFound, "no node");

        // IfExist, optional
        bool ifExist = false;
        if (interContext.Cursor.TryPeekValue(out var ifExistToken) && ifExistToken.Token.Value == "ifexist")
        {
            interContext.Cursor.MoveNext();
            ifExist = true;
        }

        // key={key}
        if (!InterLangTool.TryGetValue(interContext, "key", out var nodeKey)) return (StatusCode.NotFound, "Cannot find key=node");

        // Set
        Dictionary<string, string?>? tags = null;
        Dictionary<string, string>? data = null;

        var tagsAndData = InterLangTool.GetTagsAndData(interContext);
        if (tagsAndData.IsOk())
        {
            tags = tagsAndData.Value.Tags;
            data = tagsAndData.Value.Data;
        }

        if (!interContext.Cursor.TryGetValue(out var term) || term.Token.Value != ";") return (StatusCode.NotFound, "term ';'");

        scope.Cancel();
        return new GiNode
        {
            ChangeType = changeType,
            Key = nodeKey,
            Tags = tags?.ToImmutableDictionary() ?? ImmutableDictionary<string, string?>.Empty,
            Data = data?.ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty,
            IfExist = ifExist,
        };
    }

    public static IReadOnlyList<GraphLinkData> GetLinkData(this GiNode subject) => subject.Data
        .Select(x => new GraphLinkData
        {
            NodeKey = subject.Key,
            Name = x.Key,
            FileId = GraphTool.CreateFileId(subject.Key, x.Key),
            Data = Convert.FromBase64String(x.Value).ToDataETag(),
        }).ToImmutableArray();
}
