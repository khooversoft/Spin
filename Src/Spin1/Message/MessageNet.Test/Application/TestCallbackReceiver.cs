using MessageNet.sdk.Endpoint;
using MessageNet.sdk.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MessageNet.Test.Application
{
    public class TestCallbackReceiver : ICallbackFactory
    {
        private readonly ConcurrentQueue<MessagePacket> _queue = new ConcurrentQueue<MessagePacket>();

        public ICallback Create(EndpointId endpointId, Uri callbackUrl) => new TestCallback(this);

        public void Send(MessagePacket messagePacket) => _queue.Enqueue(messagePacket);

        public IReadOnlyList<MessagePacket> GetMessages() => _queue.ToList();
    }

    public class TestCallback : ICallback
    {
        private readonly TestCallbackReceiver _parent;

        public TestCallback(TestCallbackReceiver parent) => _parent = parent;

        public Task<(bool ok, string? message)> Send(MessagePacket messagePacket)
        {
            _parent.Send(messagePacket);
            return Task.FromResult<(bool, string?)>((true, null));
        }
    }
}
