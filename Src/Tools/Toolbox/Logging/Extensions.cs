using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Toolbox.Tools;

namespace Toolbox.Logging
{
    public static class Extensions
    {
        public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, string loggingFolder, string baseLogFileName, int limit = 10)
        {
            builder.AddProvider(new FileLoggerProvider(loggingFolder, baseLogFileName, limit));
            return builder;
        }

        public static ILoggingBuilder AddLoggerBuffer(this ILoggingBuilder builder)
        {
            ILoggerBuffer loggingBuffer = new LoggerBuffer();
            builder.Services.AddSingleton<ILoggerBuffer>(loggingBuffer);

            builder.AddProvider(new TargetBlockLoggerProvider(loggingBuffer.TargetBlock));
            builder.AddFilter<TargetBlockLoggerProvider>(x => true);

            return builder;
        }

        public static IDisposable LogEntryExit(
            this ILogger logger,
            string? message = null,
            [CallerMemberName] string function = "",
            [CallerFilePath] string path = "",
            [CallerLineNumber] int lineNumber = 0
            )
        {
            message = message ?? "<no message>";

            logger
                .NotNull()
                .LogTrace("Enter: Message={message}, Method={method}, path={path}, line={lineNumber}", message, function, path, lineNumber);

            var sw = Stopwatch.StartNew();

            return new FinalizeScope(() =>
                logger.LogTrace("Exit: ms={ms} Method={method}, path={path}, line={lineNumber}", sw.ElapsedMilliseconds, function, path, lineNumber)
                );
        }
    }
}