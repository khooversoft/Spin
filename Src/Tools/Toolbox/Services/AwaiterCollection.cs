using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Services
{
    /// <summary>
    /// Manager to handle TCS for messages that expect responses
    /// </summary>
    public class AwaiterCollection<T> : IAwaiterCollection<T>
    {
        private readonly ConcurrentDictionary<Guid, Registration> _completion = new ConcurrentDictionary<Guid, Registration>();
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(5);

        public AwaiterCollection()
        {
        }

        public AwaiterCollection(TimeSpan defaultTimeout)
        {
            _defaultTimeout = defaultTimeout;
        }

        public void Register(Guid id, TaskCompletionSource<T> tcs, TimeSpan? timeout = null)
        {
            tcs.VerifyNotNull(nameof(tcs));

            timeout ??= _defaultTimeout;
            var cancellationTokenSource = new CancellationTokenSource((TimeSpan)timeout);
            cancellationTokenSource.Token.Register(() => SetException(id, new TimeoutException($"MessageNet: response was not received within timeout: {timeout}")));

            _completion[id] = new Registration(tcs, cancellationTokenSource);
        }

        /// <summary>
        /// Set the result on the TCS waiting for a response, id must be in the 2nd header, the response
        /// </summary>
        /// <param name="id">id of the awaiter</param>
        /// <param name="subject">data to pass</param>
        /// <returns>true for processed, false if not</returns>
        public bool SetResult(Guid id, T subject)
        {
            if (_completion.TryRemove(id, out Registration? registration))
            {
                try { registration.Tcs.SetResult(subject); }
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
            if (_completion.TryRemove(id, out Registration? registration))
            {
                try { registration.Tcs.SetException(exception); }
                finally { registration.Dispose(); }
            }
        }

        private record Registration : IDisposable
        {
            public Registration(TaskCompletionSource<T> tcs, CancellationTokenSource tokenSource)
            {
                Tcs = tcs;
                TokenSource = tokenSource;
            }

            public TaskCompletionSource<T> Tcs { get; }

            public CancellationTokenSource TokenSource { get; }

            public void Dispose() => ((IDisposable)TokenSource).Dispose();
        }
    }
}