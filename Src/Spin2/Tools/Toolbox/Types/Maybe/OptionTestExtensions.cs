using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Types;

public static class OptionTestExtensions
{
    public static Option Test(this Option option, Func<bool> test)
    {
        if (option.IsError()) return option;
        return test() ? StatusCode.OK : StatusCode.BadRequest;
    }

    public static Option Test(this Option option, Func<Option> test)
    {
        if (option.IsError()) return option;
        return test();
    }

    public static async Task<Option> TestAsync(this Task<Option> option, Func<Option> test)
    {
        var o = await option;
        if (o.IsError()) return o;

        return test();
    }

    public static async Task<Option> TestAsync(this Task<Option> option, Func<Task<Option>> test)
    {
        var o = await option;
        if (o.IsError()) return o;

        return await test();
    }
}
