using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Toolbox.Tools;

namespace Toolbox.Logging
{
    /// <summary>
    /// Memory logger with limited size.  Normally used to see the last n number of log messages with test cases.
    /// </summary>
    public class MemoryLogger : ILogger
    {
        private readonly BoundedQueue<string> _boundedQueue;

        /// <summary>
        /// Construct with max size or default
        /// </summary>
        /// <param name="maxSize"></param>
        public MemoryLogger(string name, BoundedQueue<string> boundedQueue)
        {
            Name = name;
            _boundedQueue = boundedQueue;
        }

        public IDisposable BeginScope<TState>(TState state) => null!;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _boundedQueue.Enqueue($"{Name}: " + formatter(state, exception));
        }

        /// <summary>
        /// Name of logger
        /// </summary>
        public string Name { get; }
    }

    public class MemoryLogger<T> : MemoryLogger, ILogger<T>
    {
        public MemoryLogger(string name, BoundedQueue<string> boundedQueue)
            : base(name, boundedQueue)
        {
        }
    }
}
