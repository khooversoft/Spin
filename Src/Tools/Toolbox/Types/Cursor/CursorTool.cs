using Toolbox.Extensions;

namespace Toolbox.Types;

public static class CursorTool
{
    public static Cursor<T> ToCursor<T>(this IReadOnlyList<T> collection) => new Cursor<T>(collection);

    public static IReadOnlyList<T> FromCursor<T>(this Cursor<T> subject, int maxSize)
    {
        int available = subject.List.Count - subject.Index;
        if (available <= 0) return Array.Empty<T>();

        var data = subject.List.Skip(subject.Index).Take(maxSize).ToArray();
        return data;
    }

    public static string DebugCursorLocation<T>(this Cursor<T> subject)
    {
        int startIndex = Math.Max(0, subject.Index - 5);

        return subject.List.Skip(startIndex)
            .Take(8)
            .Select((x, i) => $"{CursorTool.Quote(x?.ToString())}")
            .Prepend($"Token='{CursorTool.Quote(subject.List[subject.Index]?.ToString())}', Index={subject.Index}, startIndex={startIndex}")
            .Join(", ");
    }

    public static string Quote(string? value) => value switch
    {
        null => "<null>",
        var v => $"'{v}'",
    };
}

