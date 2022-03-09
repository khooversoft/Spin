using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Azure.Queue.RPC;

public class QueueAwaiterService : IQueueAwaiterService
{
    private readonly ConcurrentDictionary<Guid, Registration> _completion = new ConcurrentDictionary<Guid, Registration>();
    private readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(5);

    public QueueAwaiterService() { }

    public QueueAwaiterService(TimeSpan defaultTimeout) => _defaultTimeout = defaultTimeout;

    public void Add(Guid id, TaskCompletionSource<MessagePayload> tcs, TimeSpan? timeout = null)
    {
        tcs.VerifyNotNull(nameof(tcs));

        timeout ??= _defaultTimeout;

        var cancellationTokenSource = new CancellationTokenSource((TimeSpan)timeout);
        cancellationTokenSource.Token.Register(() => SetException(id, new TimeoutException($"MessageNet: response was not received within timeout: {timeout}")));

        _completion[id] = new Registration { Tcs = tcs, TokenSource = cancellationTokenSource };
    }

    /// <summary>
    /// Set the result on the TCS waiting for a response, id must be in the 2nd header, the response
    /// </summary>
    /// <param name="packet"></param>
    /// <returns>true for processed, false if not</returns>
    public bool SetResult(MessagePayload messagePayload)
    {
        if (messagePayload == null || messagePayload.CallerMessageId == null) return false;

        if (_completion.TryRemove((Guid)messagePayload.CallerMessageId, out Registration? registration))
        {
            try { registration.Tcs.SetResult(messagePayload); }
            finally { registration.Dispose(); }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Set exception on the TCS waiting for a response
    /// </summary>
    /// <param name="netMessage">original net message</param>
    /// <param name="exception">exception</param>
    public void SetException(Guid id, Exception exception)
    {
        exception.VerifyNotNull(nameof(exception));

        Registration registration;

        if (_completion.TryRemove(id, out registration!))
        {
            try { registration.Tcs.SetException(exception); }
            finally { registration.Dispose(); }
        }
    }

    private record Registration : IDisposable
    {
        public TaskCompletionSource<MessagePayload> Tcs { get; init; } = null!;

        public CancellationTokenSource TokenSource { get; init; } = null!;

        public void Dispose() => TokenSource.Dispose();
    }
}
