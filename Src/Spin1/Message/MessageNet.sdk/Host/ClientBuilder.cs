using MessageNet.sdk.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Azure.Queue;

namespace MessageNet.sdk.Host
{
    public class ClientBuilder : QueueClientBuilder<Message>
    {
        public ClientBuilder()
        {
            GetId = x => x.MessageId;
        }
    }
}
