//using System.Diagnostics;
//using Microsoft.Extensions.Logging;
//using Toolbox.Types;

//namespace Toolbox.Tools;

//public static class LoggerContextExtensions
//{
//    [DebuggerStepThrough()]
//    public static void Log(this ILoggingContext context, LogLevel logLevel, string? message, params object?[] args)
//    {
//        var record = StructureLineBuilder.Start()
//            .Add(message, args)
//            .Add(context)
//            .Build();

//        context.Context.Logger.Log(logLevel, record.Message, record.Args);
//    }

//    [DebuggerStepThrough()]
//    public static void LogInformation(this ILoggingContext context, string? message, params object?[] args)
//    {
//        var record = StructureLineBuilder.Start()
//            .Add(message, args)
//            .Add(context)
//            .Build();

//        context.Context.Logger.LogInformation(record.Message, record.Args);
//    }

//    [DebuggerStepThrough()]
//    public static void LogDebug(this ILoggingContext context, string? message, params object?[] args)
//    {
//        var record = StructureLineBuilder.Start()
//            .Add(message, args)
//            .Add(context)
//            .Build();

//        context.Context.Logger.LogDebug(record.Message, record.Args);
//    }

//    [DebuggerStepThrough()]
//    public static void LogWarning(this ILoggingContext context, string? message, params object?[] args)
//    {
//        var record = StructureLineBuilder.Start()
//            .Add(message, args)
//            .Add(context)
//            .Build();

//        context.Context.Logger.LogWarning(record.Message, record.Args);
//    }

//    [DebuggerStepThrough()]
//    public static void LogWarning(this ILoggingContext context, Exception ex, string? message, params object?[] args)
//    {
//        var record = StructureLineBuilder.Start()
//            .Add(message, args)
//            .Add(context)
//            .Build();

//        context.Context.Logger.LogWarning(ex, record.Message, record.Args);
//    }

//    [DebuggerStepThrough()]
//    public static void LogError(this ILoggingContext context, string? message, params object?[] args)
//    {
//        var record = StructureLineBuilder.Start()
//            .Add(message, args)
//            .Add(context)
//            .Build();

//        context.Context.Logger.LogError(record.Message, record.Args);
//    }

//    [DebuggerStepThrough()]
//    public static void LogError(this ILoggingContext context, Exception ex, string? message, params object?[] args)
//    {
//        var record = StructureLineBuilder.Start()
//            .Add(message, args)
//            .Add(context)
//            .Build();

//        context.Context.Logger.LogError(ex, record.Message, record.Args);
//    }

//    [DebuggerStepThrough()]
//    public static void LogCritical(this ILoggingContext context, string? message, params object?[] args)
//    {
//        var record = StructureLineBuilder.Start()
//            .Add(message, args)
//            .Add(context)
//            .Build();

//        context.Context.Logger.LogCritical(record.Message, record.Args);
//    }

//    [DebuggerStepThrough()]
//    public static void LogCritical(this ILoggingContext context, Exception ex, string? message, params object?[] args)
//    {
//        var record = StructureLineBuilder.Start()
//            .Add(message, args)
//            .Add(context)
//            .Build();

//        context.Context.Logger.LogCritical(ex, record.Message, record.Args);
//    }

//    [DebuggerStepThrough()]
//    public static void LogTrace(this ILoggingContext context, string? message, params object?[] args)
//    {
//        var record = StructureLineBuilder.Start()
//            .Add(message, args)
//            .Add(context)
//            .Build();

//        context.Context.Logger.LogTrace(record.Message, record.Args);
//    }

//    [DebuggerStepThrough()]
//    public static void LogMetric(this ILoggingContext context, string metricName, string unit, double value, string? message = null, params object?[] args)
//    {
//        var record = StructureLineBuilder.Start()
//            .Add(message, args)
//            .Add("metric:{metricName}", metricName)
//            .Add("value={value}", value)
//            .Add("unit={unit}", unit)
//            .Add(context)
//            .Build();

//        context.Context.Logger.LogDebug(record.Message, record.Args);
//    }
//}