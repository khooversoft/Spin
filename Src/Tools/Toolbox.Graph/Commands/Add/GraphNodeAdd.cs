﻿using Toolbox.Types;

namespace Toolbox.Graph;

public record GraphNodeAdd : IGraphQL
{
    public string Key { get; init; } = null!;
    public Tags Tags { get; init; } = new Tags();
    public bool Upsert { get; init; }
}