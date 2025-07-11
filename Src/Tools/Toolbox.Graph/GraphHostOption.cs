namespace Toolbox.Graph;

public record GraphHostOption
{
    public bool ShareMode { get; init; }
    public bool DisableCache { get; init; }
    public bool UseBackgroundWriter { get; init; }
}
