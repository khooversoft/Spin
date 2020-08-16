using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;
using Toolbox.Tools;

namespace Toolbox.Logging
{
    public class FileLogger : ILogger
    {
        private readonly ITargetBlock<string> _fileSync;
        private readonly string _categoryName;

        public FileLogger(ITargetBlock<string> fileSync, string categoryName)
        {
            fileSync.VerifyNotNull(nameof(fileSync));

            _fileSync = fileSync;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) => null!;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var builder = new StringBuilder();
            builder.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            builder.Append(" [");
            builder.Append(logLevel.ToString());
            builder.Append("] ");
            builder.Append(_categoryName);
            builder.Append(": ");
            builder.AppendLine(formatter(state, exception));

            if (exception != null)
            {
                builder.AppendLine(exception.ToString());
            }

            _fileSync.Post(builder.ToString());
        }
    }
}

