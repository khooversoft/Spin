namespace Toolbox.Tools;

public static class ComparerTool
{
    public static IEqualityComparer<T> ComparerFor<T>()
    {
        var result = (typeof(T) == typeof(string)) switch
        {
            true => (IEqualityComparer<T>)StringComparer.OrdinalIgnoreCase,
            false => EqualityComparer<T>.Default,
        };

        return result;
    }

    public static IEqualityComparer<T> ComparerFor<T>(this IEqualityComparer<T>? comparer)
    {
        return comparer switch
        {
            IEqualityComparer<T> v => v,
            null => ComparerFor<T>(),
        };
    }
}
