using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Azure.Queue
{
    public record QueueReceiverOption<T> where T : class
    {
        public QueueOption QueueOption { get; init; } = null!;

        public Func<T, Task> Receiver { get; init; } = null!;

        public bool AutoComplete { get; init; } = false;

        public int MaxConcurrentCalls { get; init; } = 10;
    }
}
