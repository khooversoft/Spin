using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Tools;

public static class ComparerTool
{
    public static IEqualityComparer<T> ComparerFor<T>(this IEqualityComparer<T>? comparer)
    {
        return comparer switch
        {
            IEqualityComparer<T> v => v,
            null => (typeof(T) == typeof(string)) switch
            {
                true => (IEqualityComparer<T>)StringComparer.OrdinalIgnoreCase,
                false => EqualityComparer<T>.Default,
            }
        };
    }
}
