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
    private IndexScope<T>? _indexScope;

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

    // Scope stack for cursor index
    public IndexScope<T> IndexScope => _indexScope ??= new IndexScope<T>(this);

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
    /// Move cursor to next index
    /// </summary>
    /// <returns>if in range, true, if not false</returns>
    public bool MoveNext()
    {
        if (_list.Count == 0) return false;

        int current = Math.Max(Math.Min(Interlocked.Increment(ref _cursor), _list.Count), 0);
        if (current >= _list.Count) return false;

        return true;
    }

    /// <summary>
    /// Try to get the next value and increment cursor if value is returned
    /// </summary>
    /// <param name="value">value to return</param>
    /// <returns>true if returned value, false if not</returns>
    public bool TryGetValue([MaybeNullWhen(returnValue: false)] out T value)
    {
        value = default;
        if (!MoveNext()) return false;

        value = Current;
        return true;
    }

    /// <summary>
    /// Get next value
    /// </summary>
    /// <returns>Option with hasValue set</returns>
    public Option<T> NextValue()
    {
        bool hasValue = TryGetValue(out T? value);
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

    public T? PeekValueString => TryPeekValue(out T? value) ? value : default;

    public string PeeKValuesToString => PeekValues();
    public string GetDebuggerDisplay() => $"Cursor: Index={Index}, _list.Count={_list.Count}, Current={Current?.ToString() ?? "<null>"}, Peek= {PeekValues()}";
    private int Track(int value) => value.Action(x => MaxIndex = Math.Max(MaxIndex, x));
}
