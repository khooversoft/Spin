using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Extensions;

public static class EnumerableCompareExtensions
{
    public static bool IsEquivalent<T>(this IEnumerable<T> subject, IEnumerable<T> value, IEqualityComparer<T>? comparer = null)
    {
        subject.NotNull();
        value.NotNull();
        comparer ??= EqualityComparer<T>.Default;

        var subjectCount = getCount(subject);
        var valueCount = getCount(value);
        if (subjectCount != valueCount) return false;

        var hashSet = new HashSet<T>(subject, comparer);

        foreach (var item in value)
        {
            if (hashSet.Contains(item) == false) return false;
        }

        return true;

        int getCount(IEnumerable<T> list) => list switch
        {
            T[] v => v.Length,
            ICollection<T> v => v.Count,
            _ => list.Count(),
        };
    }

    public static bool IsNotEquivalent<T>(this IEnumerable<T> subject, IEnumerable<T> value, IEqualityComparer<T>? comparer = null)
    {
        var result = subject.IsEquivalent(value, comparer);
        return !result;
    }

    public static IReadOnlyList<T> DifferenceAre<T>(this IEnumerable<T> subject, IEnumerable<T> value, IEqualityComparer<T>? comparer = null)
    {
        subject.NotNull();
        value.NotNull();
        comparer ??= EqualityComparer<T>.Default;

        var hashSet = new HashSet<T>(subject, comparer);
        var returnList = new Sequence<T>();

        foreach (var item in value)
        {
            if (hashSet.Contains(item) == false) returnList += item;
        }

        return returnList;
    }
}
