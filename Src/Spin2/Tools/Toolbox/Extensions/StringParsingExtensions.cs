using System.Diagnostics;

namespace Toolbox.Extensions;

public static class StringParsingExtensions
{

    /// <summary>
    /// Has Tag, string format=[tag][;...]
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static bool HasTag(this string? subject, string tag) => (subject ?? string.Empty)
        .Split(';', StringSplitOptions.RemoveEmptyEntries)
        .Where(x => x.EqualsIgnoreCase(tag))
        .Select(x => true)
        .FirstOrDefault(false);

    [DebuggerStepThrough]
    public static char? GetFirstChar(this string? subject) => subject switch
    {
        string v when v.Length > 0 => v[0],
        _ => null,
    };

    [DebuggerStepThrough]
    public static char? GetLastChar(this string? subject) => subject switch
    {
        string v when v.Length > 1 => v[^1],
        _ => null,
    };

    [DebuggerStepThrough]
    public static string? GetMiddleChars(this string? subject) => subject switch
    {
        string v when v.Length > 2 => v[1..^1],
        _ => null,
    };
}
