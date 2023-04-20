using System.Diagnostics;

namespace Toolbox.Tools.Zip;

[DebuggerDisplay("Source={Source}, Destination={Destination}")]
public record CopyTo
{
    public string Source { get; init; } = null!;

    public string Destination { get; init; } = null!;
}