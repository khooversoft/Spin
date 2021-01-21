using MessageNet.sdk.Models;
using MessageNet.sdk.Protocol;
using System;
using System.Threading.Tasks;
using Toolbox.Azure.Queue;

namespace MessageNet.sdk.Host
{
    public interface IMessageHost
    {
        Task<MessagePacket> Call(MessagePacket messagePacket);
        QueueClient<MessagePacket> GetClient(string endpointId);
        MessageHost Register(params MessageNodeOption[] messageNodeOptions);
        Task Send(MessagePacket messagePacket);
        void StartReceiver(string endpointId, Func<MessagePacket, Task> receiver);
        Task StopReceiver(string endpointId);
    }
}