using System;
using System.Threading.Tasks;

namespace Toolbox.Azure.Queue
{
    public interface IQueueReceiverFactory
    {
        IQueueReceiver Create<T>(QueueOption queueOption, Func<T, Task> receiver) where T : class;
    }
}