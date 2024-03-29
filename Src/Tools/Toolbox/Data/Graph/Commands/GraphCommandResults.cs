﻿namespace Toolbox.Data;

public record GraphCommandResults
{
    public IReadOnlyList<GraphQueryResult> Items { get; init; } = Array.Empty<GraphQueryResult>();
}

