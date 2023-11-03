﻿namespace Toolbox.Data;

public record GraphNodeQuery<TKey> : IGraphQL where TKey : notnull
{
    public TKey? Key { get; init; }
    public string? Tags { get; init; }
    public string? Alias { get; init; }
}


public static class GraphNodeQueryExtensions
{
    public static bool IsMatch(this GraphNodeQuery<string> subject)
    {
        bool isKey = Key == null || Key.IsMatch(node.Key);
        bool isTag = Tags == null || node.Tags.Has(Tags);

        return isKey && isTag;
    }
}