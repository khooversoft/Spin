﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

public static class ScopeExtensions
{
    public static void Log(this ScopeContextLocation context, LogLevel logLevel, string? message, params object?[] args)
    {
        message = ConstructMessage(message);
        object[] newObjects = AddContext(args, context);

        context.Context.Logger.Log(logLevel, message, newObjects);
    }

    public static void LogInformation(this ScopeContextLocation context, string? message, params object?[] args)
    {
        message = ConstructMessage(message);
        object[] newObjects = AddContext(args, context);

        context.Context.Logger.LogInformation(message, newObjects);
    }

    public static void LogWarning(this ScopeContextLocation context, string? message, params object?[] args)
    {
        message = ConstructMessage(message);
        object[] newObjects = AddContext(args, context);

        context.Context.Logger.LogWarning(message, newObjects);
    }

    public static void LogWarning(this ScopeContextLocation context, Exception ex, string? message, params object?[] args)
    {
        message = ConstructMessage(message);
        object[] newObjects = AddContext(args, context);

        context.Context.Logger.LogWarning(ex, message, newObjects);
    }

    public static void LogError(this ScopeContextLocation context, string? message, params object?[] args)
    {
        message = ConstructMessage(message);
        object[] newObjects = AddContext(args, context);

        context.Context.Logger.LogError(message, newObjects);
    }

    public static void LogError(this ScopeContextLocation context, Exception ex, string? message, params object?[] args)
    {
        message = ConstructMessage(message);
        object[] newObjects = AddContext(args, context);

        context.Context.Logger.LogError(ex, message, newObjects);
    }

    public static void LogCritical(this ScopeContextLocation context, string? message, params object?[] args)
    {
        message = ConstructMessage(message);
        object[] newObjects = AddContext(args, context);

        context.Context.Logger.LogCritical(message, newObjects);
    }

    public static void LogCritical(this ScopeContextLocation context, Exception ex, string? message, params object?[] args)
    {
        message = ConstructMessage(message);
        object[] newObjects = AddContext(args, context);

        context.Context.Logger.LogCritical(ex, message, newObjects);
    }

    public static void LogTrace(this ScopeContextLocation context, string? message, params object?[] args)
    {
        message = ConstructMessage(message);
        object[] newObjects = AddContext(args, context);

        context.Context.Logger.LogTrace(message, newObjects);
    }

    public static IDisposable LogEntryExit(this ScopeContextLocation context, string? message = null, params object?[] args)
    {
        log("ScopeEnter", message, args);

        var sw = Stopwatch.StartNew();

        return new FinalizeScope<ScopeContextLocation>(context, x =>
        {
            log("ScopeExit", message, args);
        });

        void log(string header, string? message, object?[] args)
        {
            message = ConstructMessage($"[{header}] " + message);
            object[] newObjects = AddContext(args, context);
            context.Context.Logger.LogInformation(message, newObjects);
        }
    }

    private static string ConstructMessage(string? message) => message switch
    {
        null => string.Empty,
        string v => v + ", ",
    } +
    "traceId={traceId}, " +
    "callerFunction={callerFunction}, " +
    "callerFilePath={callerFilePath}, " +
    "callerLineNumber={callerLineNumber}";

    private static object[] AddContext(object?[] args, ScopeContextLocation context) => (object[])(args ?? Array.Empty<object?>())
        .Select(x => (x?.GetType().IsClass == true) switch
        {
            true => x.ToJsonPascalSafe(context.Context),
            false => x
        })
        .Append(context.Context.TraceId)
        .Append(context.Location.CallerFunction)
        .Append(context.Location.CallerFilePath)
        .Append(context.Location.CallerLineNumber)
        .ToArray();
}
