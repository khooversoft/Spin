using System.Diagnostics;
using System.Security.Cryptography;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Extensions;

public static class EnumerableExtensions
{
    /// <summary>
    /// Convert a scalar value to enumerable
    /// </summary>
    /// <typeparam name="T">type</typeparam>
    /// <param name="self">object to convert</param>
    /// <returns>enumerator</returns>
    [DebuggerStepThrough]
    public static IEnumerable<T> ToEnumerable<T>(this T self)
    {
        yield return self;
    }

    /// <summary>
    /// Execute 'action' on each item
    /// </summary>
    /// <typeparam name="T">type</typeparam>
    /// <param name="subjects">types to process</param>
    /// <param name="action">action to execute</param>
    public static void ForEach<T>(this IEnumerable<T> subjects, Action<T> action)
    {
        subjects.NotNull();
        action.NotNull();

        foreach (var item in subjects)
        {
            action(item);
        }
    }

    /// <summary>
    /// Execute 'action' on each item
    /// </summary>
    /// <typeparam name="T">type</typeparam>
    /// <param name="subjects">list to operate on</param>
    /// <param name="action">action to execute</param>
    public static void ForEach<T>(this IEnumerable<T> subjects, Action<T, int> action)
    {
        subjects.NotNull();
        action.NotNull();

        int index = 0;
        foreach (var item in subjects)
        {
            action(item, index++);
        }
    }

    /// <summary>
    /// Covert enumerable to stack, null will return empty stack
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="subjects"></param>
    /// <returns>Stack<typeparamref name="T"/></returns>
    [DebuggerStepThrough]
    public static Stack<T> ToStack<T>(this IEnumerable<T>? subjects) => new Stack<T>(subjects ?? Array.Empty<T>());

    /// <summary>
    /// Shuffle list based on random crypto provider
    /// </summary>
    /// <typeparam name="T">type in list</typeparam>
    /// <param name="self">list to shuffle</param>
    /// <returns>shuffled list</returns>
    public static IReadOnlyList<T> Shuffle<T>(this IEnumerable<T> self)
    {
        self.NotNull();

        var list = self.ToList();

        var provider = RandomNumberGenerator.Create();
        int n = list.Count;

        while (n > 1)
        {
            var box = new byte[1];
            do
            {
                provider.GetBytes(box);
            }
            while (!(box[0] < n * (Byte.MaxValue / n)));

            var k = (box[0] % n);
            n--;

            var value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        return list;
    }

    public static IEnumerable<T> ToSafe<T>(this IEnumerable<T>? list) => (list ?? Array.Empty<T>());


    /// <summary>
    /// Insert separator between items in sequence
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static IEnumerable<T> SequenceJoin<T>(this IEnumerable<T> values, T separator)
    {
        bool run = false;

        foreach (var item in values)
        {
            if (run) yield return separator;

            run = true;
            yield return item;
        }
    }

    /// <summary>
    /// Sequence join - separator is append to all but last element
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values"></param>
    /// <param name="separatorSelect"></param>
    /// <returns></returns>
    public static IEnumerable<T> SequenceJoin<T>(this IEnumerable<T> values, Func<T, T> separatorSelect)
    {
        bool hasValue = false;
        T save = default!;

        foreach (var item in values)
        {
            if (hasValue) yield return separatorSelect(save);

            hasValue = true;
            save = item;
        }

        if (hasValue) yield return save;
    }

    /// <summary>
    /// Collection with index to be used with ForEach(...)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <returns></returns>
    public static IEnumerable<(T Item, int Index)> WithIndex<T>(this IEnumerable<T> source) => source
        .NotNull()
        .Select((item, index) => (item, index));


    public static Option<T> FirstOrDefaultOption<T>(this IEnumerable<T> source, bool returnNotFound = false)
    {
        foreach (T element in source) return element;
        return returnNotFound ? StatusCode.NotFound : Option<T>.None;
    }

    public static Option<T> FirstOrDefaultOption<T>(this IEnumerable<T> source, Func<T, bool> predicate, bool returnNotFound = false)
    {
        foreach (T element in source)
        {
            if (predicate(element)) return element;
        }

        return returnNotFound ? StatusCode.NotFound : Option<T>.None;
    }

    public static Option<T> LastOrDefaultOption<T>(this IEnumerable<T> source)
    {
        using (IEnumerator<T> e = source.GetEnumerator())
        {
            if (e.MoveNext())
            {
                T result;
                do
                {
                    result = e.Current;
                }
                while (e.MoveNext());

                return new Option<T>(true, result);
            }
        }

        return Option<T>.None;
    }
}
