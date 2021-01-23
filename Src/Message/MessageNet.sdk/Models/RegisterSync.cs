using MessageNet.sdk.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageNet.sdk.Models
{
    public record RegisterSync
    {
        public EndpointId EndpointId { get; init; } = null!;

        public string CallbackUri { get; init; } = null!;
    }
}
