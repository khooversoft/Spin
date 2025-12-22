using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Toolbox.Extensions;

namespace Toolbox;

public static partial class PathValidator
{
    /// <summary>
    /// Validates whether the given path is a valid Azure Data Lake path.
    /// 
    /// Letters: A–Z, a–z
    /// Numbers: 0–9
    /// Special characters: -, _, ., !, ~, ', (, ), *
    /// Forward slash /: Used as a path separator between directories
    /// 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPathValid(string path) => path.IsEmpty() switch
    {
        true => false,
        false => AdlsPathRegex().IsMatch(path),
    };

    // This generates the regex at compile time for better performance and AOT compatibility
    [GeneratedRegex(@"^([a-zA-Z0-9._-]+\/)*[a-zA-Z0-9._-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex AdlsPathRegex();
}
