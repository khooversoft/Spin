using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Toolbox.Extensions;

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

        //public static IDisposable LocationScope(this ILogger logger,
        //    string? message = null,
        //    [CallerMemberName] string function = "",
        //    [CallerFilePath] string path = "",
        //    [CallerLineNumber] int lineNumber = 0)
        //{
        //    var location = new { Function = function, Path = path, LineNumber = lineNumber, Message = message };
        //    return logger.BeginScope(location);
        //}
    }
}
