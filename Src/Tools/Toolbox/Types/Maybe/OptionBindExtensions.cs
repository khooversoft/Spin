using Toolbox.Tools;

namespace Toolbox.Types;

public static class OptionBindExtensions
{
    public static Option Bind(this Option subject, Func<Option> func)
    {
        func.NotNull();

        return subject.IsOk() switch
        {
            true => func(),
            false => subject,
        };
    }

    public static async Task<Option> BindAsync(this Option subject, Func<Task<Option>> func)
    {
        func.NotNull();

        return subject.IsOk() switch
        {
            true => await func(),
            false => subject,
        };
    }
}
