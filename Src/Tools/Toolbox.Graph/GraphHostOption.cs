namespace Toolbox.Graph;

public record GraphHostOption
{
    public bool ReadOnly { get; init; }
    public bool DisableCache { get; init; }
}
