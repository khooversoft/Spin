using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Types;

public static class OptionLoggingExtensions
{
    public static Option LogResult(this Option subject, ScopeContextLocation location)
    {
        location.Context.Logger.NotNull();
        if (subject.StatusCode.IsOk()) return subject;

        location.LogError("Option = IsError, statusCode={statusCode}, error={error}", subject.StatusCode, subject.Error);
        return subject;
    }
    
    public static async Task<Option> LogResult(this Task<Option> subject, ScopeContextLocation location)
    {
        location.Context.Logger.NotNull();
        
        Option option = await subject;
        if (option.StatusCode.IsOk()) return option;

        location.LogError("Option = IsError, statusCode={statusCode}, error={error}", option.StatusCode, option.Error);
        return option;
    }

    public static Option<T> LogResult<T>(this Option<T> subject, ScopeContextLocation location)
    {
        location.Context.Logger.NotNull();
        if (subject.StatusCode.IsOk()) return subject;

        location.LogError("Option = IsError, statusCode={statusCode}, error={error}", subject.StatusCode, subject.Error);
        return subject;
    }

    public static async Task<Option<T>> LogResult<T>(this Task<Option<T>> subject, ScopeContextLocation location)
    {
        location.Context.Logger.NotNull();

        Option<T> option = await subject;
        if (option.StatusCode.IsOk()) return option;

        location.LogError("Option = IsError, statusCode={statusCode}, error={error}", option.StatusCode, option.Error);
        return option;
    }
}
