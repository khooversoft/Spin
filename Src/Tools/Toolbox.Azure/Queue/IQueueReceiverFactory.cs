using System;
using System.Threading.Tasks;

namespace Toolbox.Azure.Queue
{
    public interface IQueueReceiverFactory
    {
        IQueueReceiver Create<T>(QueueReceiverOption<T> queueReceiver) where T : class;
    }
}