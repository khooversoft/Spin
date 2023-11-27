using System.Diagnostics;

namespace Toolbox.Types;

[DebuggerDisplay("StatusCode={Option.StatusCode}, Error={Option.Error}")]
public class OptionTest
{
    public Option Option { get; private set; } = StatusCode.OK;

    public StatusCode StatusCode => Option.StatusCode;
    public string? Error => Option.Error;

    public OptionTest Test<T>(Option<T> test)
    {
        if (Option.IsError()) return this;

        Option = test.ToOptionStatus();
        return this;
    }

    public OptionTest Test(Func<bool> test, StatusCode failedStatusCode = StatusCode.BadRequest, string? error = null)
    {
        if (Option.IsError()) return this;

        Option = test() switch
        {
            true => StatusCode.OK,
            false => new Option(failedStatusCode, error),
        };

        return this;
    }

    public OptionTest Test(Func<Option> test)
    {
        if (Option.IsError()) return this;

        Option = test();
        return this;
    }

    public async Task<OptionTest> TestAsync(Func<Task<Option>> test)
    {
        if (Option.IsError()) return this;

        Option = await test();
        return this;
    }

    public static implicit operator Option(OptionTest subject) => subject.Option;
}


public static class OptionTestExtension
{
    public static bool IsOk(this OptionTest subject) => subject.Option.IsOk();
    public static bool IsNotFound(this OptionTest subject) => subject.Option.IsNotFound();
    public static bool IsError(this OptionTest subject) => subject.Option.IsError();
}