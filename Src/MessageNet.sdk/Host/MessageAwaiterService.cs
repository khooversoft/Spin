using MessageNet.sdk.Protocol;
using MessageNet.sdk.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Azure.Queue;
using Toolbox.Tools;

namespace MessageNet.sdk.Host
{
    /// <summary>
    /// Manager to handle TCS for messages that expect responses
    /// </summary>
    public class MessageAwaiter : QueueAwaiterService
    {
        public MessageAwaiter() { }

        public MessageAwaiter(TimeSpan defaultTimeout) : base(defaultTimeout) { }

        public void Add(Guid id, TaskCompletionSource<Packet> tcs, TimeSpan? timeout = null)
        {
            tcs.VerifyNotNull(nameof(tcs));

            timeout ??= _defaultTimeout;

            var cancellationTokenSource = new CancellationTokenSource((TimeSpan)timeout);
            cancellationTokenSource.Token.Register(() => SetException(id, new TimeoutException($"MessageNet: response was not received within timeout: {timeout.ToString()}")));

            _completion[id] = new Registration(tcs, cancellationTokenSource);
        }

        /// <summary>
        /// Set the result on the TCS waiting for a response, id must be in the 2nd header, the response
        /// </summary>
        /// <param name="packet"></param>
        /// <returns>true for processed, false if not</returns>
        public bool SetResult(Packet packet)
        {
            packet.VerifyNotNull(nameof(packet));

            Message? message = packet.GetFromMessage();
            if (message == null) return false;

            if (_completion.TryRemove(message.MessageId, out Registration? registration))
            {
                try { registration.Tcs.SetResult(packet); }
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
            public Registration(TaskCompletionSource<Packet> tcs, CancellationTokenSource tokenSource)
            {
                Tcs = tcs;
                TokenSource = tokenSource;
            }

            public TaskCompletionSource<Packet> Tcs { get; }

            public CancellationTokenSource TokenSource { get; }

            public void Dispose() => TokenSource.Dispose();
        }
    }
}
