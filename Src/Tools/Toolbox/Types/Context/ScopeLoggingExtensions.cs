//using Microsoft.Extensions.Logging;

//namespace Toolbox.Types;

//public static class ScopeLoggingExtensions
//{
//    public static void Log(this ScopeContextLocation context, LogLevel logLevel, string? message, params object?[] args)
//    {
//        message = context.Formatter.ConstructMessage(message);
//        object[] newObjects = context.Formatter.AddContext(args, context);

//        context.Context.Logger.Log(logLevel, message, newObjects);
//    }

//    public static void LogStatus(this ScopeContextLocation context, Option option, string message, params object?[] args)
//    {
//        const string fmt = "statusCode={statusCode}, error={error}";

//        (message, args) = option switch
//        {
//            var v when v.IsOk() => (message, args),

//            var v => (
//                    message == null ? fmt : message += ", " + fmt,
//                    args.Concat(new object?[] { v.StatusCode, v.Error ?? "< no error >" }).ToArray()
//                ),
//        };

//        message = context.Formatter.ConstructMessage(message);
//        object[] newObjects = context.Formatter.AddContext(args, context);

//        context.Context.Logger.Log(option.IsOk() ? LogLevel.Information : LogLevel.Error, message, newObjects);
//    }

//    public static void LogInformation(this ScopeContextLocation context, string? message, params object?[] args)
//    {
//        message = context.Formatter.ConstructMessage(message);
//        object[] newObjects = context.Formatter.AddContext(args, context);

//        context.Context.Logger.LogInformation(message, newObjects);
//    }

//    public static void LogWarning(this ScopeContextLocation context, string? message, params object?[] args)
//    {
//        message = context.Formatter.ConstructMessage(message);
//        object[] newObjects = context.Formatter.AddContext(args, context);

//        context.Context.Logger.LogWarning(message, newObjects);
//    }

//    public static void LogWarning(this ScopeContextLocation context, Exception ex, string? message, params object?[] args)
//    {
//        message = context.Formatter.ConstructMessage(message);
//        object[] newObjects = context.Formatter.AddContext(args, context);

//        context.Context.Logger.LogWarning(ex, message, newObjects);
//    }

//    public static void LogError(this ScopeContextLocation context, string? message, params object?[] args)
//    {
//        message = context.Formatter.ConstructMessage(message);
//        object[] newObjects = context.Formatter.AddContext(args, context);

//        context.Context.Logger.LogError(message, newObjects);
//    }

//    public static void LogError(this ScopeContextLocation context, Exception ex, string? message, params object?[] args)
//    {
//        message = context.Formatter.ConstructMessage(message);
//        object[] newObjects = context.Formatter.AddContext(args, context);

//        context.Context.Logger.LogError(ex, message, newObjects);
//    }

//    public static void LogCritical(this ScopeContextLocation context, string? message, params object?[] args)
//    {
//        message = context.Formatter.ConstructMessage(message);
//        object[] newObjects = context.Formatter.AddContext(args, context);

//        context.Context.Logger.LogCritical(message, newObjects);
//    }

//    public static void LogCritical(this ScopeContextLocation context, Exception ex, string? message, params object?[] args)
//    {
//        message = context.Formatter.ConstructMessage(message);
//        object[] newObjects = context.Formatter.AddContext(args, context);

//        context.Context.Logger.LogCritical(ex, message, newObjects);
//    }

//    public static void LogTrace(this ScopeContextLocation context, string? message, params object?[] args)
//    {
//        message = context.Formatter.ConstructMessage(message);
//        object[] newObjects = context.Formatter.AddContext(args, context);

//        context.Context.Logger.LogTrace(message, newObjects);
//    }


//    //public static IDisposable LogEntryExit(this ScopeContextLocation context, string? message = null, params object?[] args)
//    //{
//    //    log("ScopeEnter", message, args);

//    //    //var sw = Stopwatch.StartNew();

//    //    return new FinalizeScope<ScopeContextLocation>(context, x =>
//    //    {
//    //        log("ScopeExit", message, args);
//    //    });

//    //    void log(string header, string? message, object?[] args)
//    //    {
//    //        message = context.Formatter.ConstructMessage($"[{header}] " + message);
//    //        object[] newObjects = context.Formatter.AddContext(args, context);
//    //        context.Context.Logger.LogTrace(message, newObjects);
//    //    }
//    //}
//}
