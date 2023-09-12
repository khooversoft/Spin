using System.Diagnostics;

namespace Toolbox.Tools.Zip;

[DebuggerDisplay("Source={Source}, Destination={Destination}")]
public readonly record struct CopyTo
{
    public string Source { get; init; }

    public string Destination { get; init; }
}