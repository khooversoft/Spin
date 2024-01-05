namespace Toolbox.Extensions;

public static class ReadOnlySpanExtensions
{
    public static IEnumerable<ReadOnlyMemory<T>> Split<T>(this ReadOnlyMemory<T> source, T delimiter) where T : IEquatable<T>
    {
        int startIndex = 0;
        int index;

        while ((index = source.Span[startIndex..].IndexOf(delimiter)) != -1)
        {
            yield return source.Slice(startIndex, index);
            startIndex += index + 1;
        }

        if (startIndex < source.Length)
        {
            yield return source[startIndex..];
        }
    }
}
