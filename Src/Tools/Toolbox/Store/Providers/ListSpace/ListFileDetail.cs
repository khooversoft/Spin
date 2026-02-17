using Toolbox.Types;

namespace Toolbox.Store;

public record ListFileDetail<T>
{
    public StorePathDetail StorePathDetail { get; init; } = null!;
    public DataETag Data { get; init; } = null!;
    public IReadOnlyList<T> Items { get; init; } = null!;
}
