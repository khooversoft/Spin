using Toolbox.Tools;

namespace Toolbox.Types;

public static class OptionBind_T_Extensions
{
    public static Option<TO> Bind<TO, T>(this Option<T> subject, Func<T, Option<TO>> func)
    {
        func.NotNull();

        return subject.HasValue switch
        {
            true => func(subject.Return()),
            false => subject.ToOptionStatus<TO>(),
        };
    }

    public static async Task<Option<TO>> BindAsync<TO, T>(this Option<T> subject, Func<T, Task<Option<TO>>> func)
    {
        func.NotNull();

        return subject.HasValue switch
        {
            true => await func(subject.Return()),
            false => subject.ToOptionStatus<TO>(),
        };
    }

    public static Option<TO> Bind<TO, T>(this Option<T> subject, Func<T, TO> func)
    {
        func.NotNull();

        return subject.HasValue switch
        {
            true => func(subject.Return()).ToOption(),
            false => subject.ToOptionStatus<TO>(),
        };
    }

    public static async Task<Option<TO>> BindAsync<TO, T>(this Task<Option<T>> subject, Func<T, Task<TO>> func)
    {
        func.NotNull();

        Option<T> subjectResult = await subject;

        return subjectResult.HasValue switch
        {
            true => await func(subjectResult.Return()),
            false => subjectResult.ToOptionStatus<TO>(),
        };
    }
}
