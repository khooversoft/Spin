using System;
using System.Threading.Tasks;

namespace Toolbox.Azure.Queue.RPC;

public interface IQueueAwaiterService
{
    void Add(Guid id, TaskCompletionSource<MessagePayload> tcs, TimeSpan? timeout = null);
    void SetException(Guid id, Exception exception);
    bool SetResult(MessagePayload messagePayload);
}
