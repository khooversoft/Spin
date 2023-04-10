using System;
using System.Threading.Tasks;

namespace Toolbox.Services
{
    public interface IAwaiterCollection<T>
    {
        bool Register(Guid id, TaskCompletionSource<T> tcs, TimeSpan? timeout = null);

        void SetException(Guid id, Exception exception);

        bool SetResult(Guid id, T packet);
    }
}