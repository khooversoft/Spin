using System;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Tools;

public record Batch<T> where T : class
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
}

public record BatchResult<T> where T : class
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string? ReferenceId { get; init; }
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
}


public static class BatchExtensions
{
    public static Batch<T> Add<T>(this Batch<T> batch, params T[] items) where T : class =>
        batch.NotNull() with
        {
            Items = (batch?.Items ?? Array.Empty<T>())
                .Concat(items ?? Array.Empty<T>())
                .ToList()
        };

    public static BatchResult<T> Add<T>(this BatchResult<T> batchResult, params T[] items) where T : class =>
        batchResult.NotNull() with
        {
            Items = (batchResult?.Items ?? Array.Empty<T>())
                .Concat(items ?? Array.Empty<T>())
                .ToList()
        };
}