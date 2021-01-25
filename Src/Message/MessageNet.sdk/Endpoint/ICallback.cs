using MessageNet.sdk.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageNet.sdk.Endpoint
{
    public interface ICallback
    {
        Task<(bool ok, string? message)> Send(MessagePacket messagePacket);
    }
}
