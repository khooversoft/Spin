using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Tools;

namespace Toolbox.Logging
{
    public class MemoryLoggerProvider : ILoggerProvider
    {
        private readonly BoundedQueue<string> _queue;

        public MemoryLoggerProvider(BoundedQueue<string> queue)
        {
            queue.VerifyNotNull(nameof(queue));

            _queue = queue;
        }

        public ILogger CreateLogger(string categoryName) => new MemoryLogger(categoryName, _queue);

        public void Dispose() { }

        public ILogger<T> CreateLogger<T>() => new MemoryLogger<T>(typeof(T).Name, _queue);
    }
}
