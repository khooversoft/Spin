using Microsoft.Extensions.Logging;
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
        private readonly ILogger<AwaiterCollection<T>> _logger;

        public AwaiterCollection(ILogger<AwaiterCollection<T>> logger)
        {
            _logger = logger;
        }

        public AwaiterCollection(TimeSpan defaultTimeout, ILogger<AwaiterCollection<T>> logger)
        {
            _defaultTimeout = defaultTimeout;
            _logger = logger;
        }

        public bool Register(Guid id, TaskCompletionSource<T> tcs, TimeSpan? timeout = null)
        {
            tcs.VerifyNotNull(nameof(tcs));

            bool added = false;

            _ = _completion.GetOrAdd(id, x =>
            {
                added = true;
                _logger.LogInformation($"{nameof(Register)}: id={id}");

                timeout ??= _defaultTimeout;
                var cancellationTokenSource = new CancellationTokenSource((TimeSpan)timeout);
                cancellationTokenSource.Token.Register(() => SetException(id, new TimeoutException($"MessageNet: response was not received within timeout: {timeout}")));

                return new Registration(tcs, cancellationTokenSource);
            });

            return added;
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
                _logger.LogInformation($"{nameof(SetResult)}: id={id}");

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
                _logger.LogInformation($"{nameof(SetException)}: id={id}, ex={exception}");

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