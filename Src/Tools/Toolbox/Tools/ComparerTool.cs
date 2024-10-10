namespace Toolbox.Tools;

public static class ComparerTool
{
    public static IEqualityComparer<T> EqualityComparerFor<T>()
    {
        var result = (typeof(T) == typeof(string)) switch
        {
            true => (IEqualityComparer<T>)StringComparer.OrdinalIgnoreCase,
            false => EqualityComparer<T>.Default,
        };

        return result;
    }

    public static IEqualityComparer<T> EqualityComparerFor<T>(this IEqualityComparer<T>? comparer)
    {
        return comparer switch
        {
            IEqualityComparer<T> v => v,
            null => EqualityComparerFor<T>(),
        };
    }

    public static IComparer<T> ComparerFor<T>()
    {
        var result = (typeof(T) == typeof(string)) switch
        {
            true => (IComparer<T>)StringComparer.OrdinalIgnoreCase,
            false => Comparer<T>.Default,
        };

        return result;
    }

    public static IComparer<T> ComparerFor<T>(this IComparer<T>? comparer)
    {
        return comparer switch
        {
            IComparer<T> v => v,
            null => ComparerFor<T>(),
        };
    }
}
