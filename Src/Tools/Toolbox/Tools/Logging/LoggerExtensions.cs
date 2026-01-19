//using Microsoft.Extensions.Logging;
//using Toolbox.Types;

//namespace Toolbox.Tools;

//public static class LoggerExtensions
//{
//    public static Option LogStatus(this Option option, ILogger logger, string message, params IEnumerable<object> args)
//    {
//        logger.NotNull();
//        message.NotNull();

//        var result = StructureLineBuilder.Start()
//            .Add(message, args)
//            .Add(option)
//            .Build();

//        LogLevel logLevel = option.StatusCode.IsOk() ? LogLevel.Debug : LogLevel.Error;
//        logger.Log(logLevel, result.Message, result.Args);

//        return option;
//    }

//    public static Option<T> LogStatus<T>(this Option<T> option, ILogger logger, string message, params IEnumerable<object> args)
//    {
//        option.ToOptionStatus().LogStatus(logger, message, args);
//        return option;
//    }
//}
