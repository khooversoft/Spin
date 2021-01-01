using MessageNet.sdk.Protocol;
using System;
using System.Threading.Tasks;

namespace MessageNet.sdk.Services
{
    public interface IMessageAwaiterService
    {
        void Add(Guid id, TaskCompletionSource<Packet> tcs, TimeSpan? timeout = null);
        void SetException(Guid id, Exception exception);
        bool SetResult(Packet packet);
    }
}