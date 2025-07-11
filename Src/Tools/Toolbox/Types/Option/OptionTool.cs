namespace Toolbox.Types;

public static class OptionTool
{
    public static Option<T> OptionSwitch<T>(bool test, Func<Option<T>> valueFunc)
    {
        if (!test) return StatusCode.BadRequest;

        var valueOption = valueFunc();
        return valueOption;
    }
}
