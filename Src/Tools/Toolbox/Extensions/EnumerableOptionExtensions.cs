using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Types;

namespace Toolbox.Extensions;

public static class EnumerableOptionExtensions
{
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
