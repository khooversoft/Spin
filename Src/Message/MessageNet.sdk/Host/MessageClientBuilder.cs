using MessageNet.sdk.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Azure.Queue;

namespace MessageNet.sdk.Host
{
    public class MessageClientBuilder : QueueClientBuilder<MessagePacket>
    {
        public MessageClientBuilder()
        {
            GetId = x => x.GetMessageId();
        }
    }
}
