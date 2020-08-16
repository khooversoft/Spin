using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Logging
{
    public static class LoggerExtensions
    {
        public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, string loggingFolder, string baseLogFileName, int limit = 10)
        {
            builder.AddProvider(new FileLoggerProvider(loggingFolder, baseLogFileName, limit));
            return builder;
        }

        public static ILoggingBuilder AddMemoryLogger(this ILoggingBuilder builder, BoundedQueue<string> queue)
        {
            builder.AddProvider(new MemoryLoggerProvider(queue));
            builder.AddFilter<MemoryLoggerProvider>(x => true);

            return builder;
        }

        public static async Task LogTrace(this HttpRequestMessage subject, ILogger logger)
        {
            const string label = "httpRequest";
            logger.LogTrace($"{label}: Uri={subject.RequestUri}, Method={subject.Method.Method}");

            if (subject.Content != null)
            {
                logger.Log(LogLevel.Trace, $"{label}: Content: {await subject.Content.ReadAsStringAsync()}");
            }

            subject.Headers.DumpHeaders(label, logger);
        }

        public static async Task LogTrace(this HttpResponseMessage subject, ILogger logger)
        {
            const string label = "httpResponse";

            await subject.RequestMessage.LogTrace(logger);

            if (subject.Content != null)
            {
                logger.Log(LogLevel.Trace, $"{label}: Content: {await subject.Content.ReadAsStringAsync()}");
            }

            subject.Headers.DumpHeaders(label, logger);
        }

        public static void DumpHeaders(this IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers, string label, ILogger logger) => headers
            .ForEach(x => logger.LogTrace($"{label}: Header {x.Key} = Value: {string.Join(", ", x.Value)}"));
    }
}