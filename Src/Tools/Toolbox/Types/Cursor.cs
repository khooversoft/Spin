using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

/// <summary>
/// Provides cursor capability to a collection
/// </summary>
/// <typeparam name="T">list type</typeparam>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class Cursor<T>
{
    private int _cursor = -1;
    private readonly IReadOnlyList<T> _list;

    /// <summary>
    /// Bind cursor to collection
    /// </summary>
    /// <param name="collection"></param>
    public Cursor(IReadOnlyList<T> collection) => _list = collection.NotNull();

    /// <summary>
    /// Reference list
    /// </summary>
    public IReadOnlyList<T> List => _list;

    /// <summary>
    /// Current cursor index (0 base)
    /// </summary>
    public int Index
    {
        get => _cursor;
        set => _cursor = Math.Max(Math.Min(value, _list.Count), -1);
    }

    /// <summary>
    /// Current value in the list pointed to by the cursor
    /// </summary>
    public T Current => _cursor >= 0 && _cursor < _list.Count ? _list[_cursor] : default!;

    /// <summary>
    /// Is cursor at the end of the collection
    /// </summary>
    public bool IsCursorAtEnd => !(_cursor >= 0 && _cursor < _list.Count);

    public int MaxIndex { get; private set; }

    /// <summary>
    /// Reset cursor to the beginning of the list
    /// </summary>
    public void Reset()
    {
        _cursor = -1;
        MaxIndex = _cursor;
    }

    /// <summary>
    /// Try to get the next value and increment cursor if value is returned
    /// </summary>
    /// <param name="value">value to return</param>
    /// <returns>true if returned value, false if not</returns>
    public bool TryNextValue([MaybeNullWhen(returnValue: false)] out T value)
    {
        value = default;
        if (_list.Count == 0) return false;

        int current = Math.Min(Interlocked.Increment(ref _cursor), _list.Count);
        if (current >= _list.Count) return false;

        value = _list[Track(current)];
        return true;
    }

    /// <summary>
    /// Get next value
    /// </summary>
    /// <returns>Option with hasValue set</returns>
    public Option<T> NextValue()
    {
        bool hasValue = TryNextValue(out T? value);
        return new Option<T>(hasValue, value!);
    }

    /// <summary>
    /// Try to get next value but do not increment the cursor
    /// </summary>
    /// <param name="value">value to return</param>
    /// <returns>true if a value is returned, false if not</returns>
    public bool TryPeekValue([MaybeNullWhen(returnValue: false)] out T value)
    {
        value = default;
        if (_list.Count == 0) return false;

        int current = Math.Min(_cursor + 1, _list.Count);
        if (current >= _list.Count) return false;

        value = _list[current];
        return true;
    }

    /// <summary>
    /// Try get next value but do not increment the cursor
    /// </summary>
    /// <returns></returns>
    public Option<T> PeekValue()
    {
        bool hasValue = TryPeekValue(out T? value);
        return new Option<T>(hasValue, value!);
    }

    /// <summary>
    /// Peek the next set of values
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public string PeekValues(int count = 5)
    {
        int current = Math.Min(_cursor + 1, _list.Count);
        int max = Math.Min(current + count, _list.Count);

        var list = _list.Skip(current)
            .Take(max - current)
            .Select(x => CursorTool.Quote(x?.ToString()))
            .Join(", ");

        return list;
    }

    public string PeekValueString => TryPeekValue(out T? value) ? value.NotNull().ToString()! : "<null>";

    public string PeeKValuesToString => PeekValues();
    public string GetDebuggerDisplay() => $"Cursor: Index={Index}, _list.Count={_list.Count}, Current={Current?.ToString() ?? "<null>"}, Peek= {PeekValues()}";
    private int Track(int value) => value.Action(x => MaxIndex = Math.Max(MaxIndex, x));


}


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
            .Select((x, i) => $"{i + startIndex}={CursorTool.Quote(x?.ToString())}")
            .Prepend($"Index={subject.Index}")
            .Join(", ");
    }

    public static string Quote(string? value) => value switch
    {
        null => "<null>",
        var v => $"'{v}'",
    };
}

