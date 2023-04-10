using System.Diagnostics.CodeAnalysis;
using Toolbox.Monads;
using Toolbox.Tools;

namespace Toolbox.Types;

/// <summary>
/// Provides cursor capability to a collection
/// </summary>
/// <typeparam name="T">list type</typeparam>
public class Cursor<T>
{
    private int _cursor = -1;
    private readonly IReadOnlyList<T> _list;

    /// <summary>
    /// Bind cursor to collection
    /// </summary>
    /// <param name="collection"></param>
    public Cursor(IReadOnlyList<T> collection)
    {
        collection.NotNull();

        _list = collection;
    }

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

    /// <summary>
    /// Reset cursor to the beginning of the list
    /// </summary>
    public void Reset() => _cursor = -1;

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

        value = _list[current];
        return true;
    }

    /// <summary>
    /// Get next value
    /// </summary>
    /// <returns>Option with hasValue set</returns>
    public Option<T> NextValue()
    {
        bool hasValue = TryNextValue(out T? value);
        return (hasValue, value!).ToOption();
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
        return (hasValue, value!).ToOption();
    }
}


public static class CursorExtensions
{
    public static Cursor<T> ToCursor<T>(this IReadOnlyList<T> collection) => new Cursor<T>(collection);

    //public static IReadOnlyList<T> FromIndex<T>(this Cursor<T> cursor, int index) =>
}

