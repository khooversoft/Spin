using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;
using Toolbox.Tools;

namespace Toolbox.Logging
{
    public class TargetBlockLoggerProvider : ILoggerProvider
    {
        private readonly ITargetBlock<string> _queue;

        public TargetBlockLoggerProvider(ITargetBlock<string> queue)
        {
            queue.NotNull();

            _queue = queue;
        }

        public ILogger CreateLogger(string categoryName) => new TargetBlockLogger(categoryName, _queue);

        public void Dispose() { }
    }
}
