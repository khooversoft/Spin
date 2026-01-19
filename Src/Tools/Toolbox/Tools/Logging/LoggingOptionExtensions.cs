using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Tools;

public static class LoggingOptionExtensions
{
    public static Option LogStatus(
        this ILogger logger,
        Option option,
        string message,
        IEnumerable<object>? args = null,
        [CallerArgumentExpression("option")] string name = ""
        )
    {
        var result = StructureLineBuilder.Start()
            .Add(message.NotEmpty(), args)
            .Add(option)
            .Add("argumentName={argumentName}", name)
            .Build();

        LogLevel logLevel = option.StatusCode.IsOk() ? LogLevel.Debug : LogLevel.Error;
        logger.Log(logLevel, result.Message, result.Args);

        return option;
    }

    public static Option<T> LogStatus<T>(
        this ILogger logger,
        Option<T> option,
        string message,
        IEnumerable<object>? args = null,
        [CallerArgumentExpression("option")] string name = ""
        )
    {
        var result = StructureLineBuilder.Start()
            .Add(message.NotEmpty(), args)
            .Add(option.ToOptionStatus())
            .Add("argumentName={argumentName}", name)
            .Build();

        LogLevel logLevel = option.StatusCode.IsOk() ? LogLevel.Debug : LogLevel.Error;
        logger.Log(logLevel, result.Message, result.Args);

        return option;
    }

    public static string ToSafeLoggingFormat(this string subject) => (subject ?? string.Empty).Replace("{", "{{").Replace("}", "}}");
}
