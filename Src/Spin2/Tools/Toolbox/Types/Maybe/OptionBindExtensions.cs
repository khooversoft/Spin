namespace Toolbox.Types.Maybe;

public static class OptionBindExtensions
{
    public static Option<TO> Bind<TO, T>(this Option<T> subject, Func<T, Option<TO>> func)
    {
        return subject.HasValue switch
        {
            true => func(subject.Return()),
            false => default,
        };
    }

    public static Option<TO> Bind<TO, T>(this Option<T> subject, Func<T, TO> func)
    {
        return subject.HasValue switch
        {
            true => func(subject.Return()).ToOption(),
            false => default,
        };
    }
}
