namespace Toolbox.Types;

public record QueryParameter
{
    public int Index { get; init; } = 0;
    public int Count { get; init; } = 1000;
    public string Domain { get; init; } = null!;
    public string? Filter { get; init; }
    public bool Recursive { get; init; }

    public static QueryParameter Default { get; } = new QueryParameter();
}
