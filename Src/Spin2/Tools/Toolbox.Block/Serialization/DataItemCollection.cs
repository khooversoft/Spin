namespace Toolbox.Block.Serialization;

public record DataItemCollection<T> where T : class
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
}
