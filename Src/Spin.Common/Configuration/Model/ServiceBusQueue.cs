using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spin.Common.Configuration.Model
{
    public record ServiceBusQueue
    {
        public string Namespace { get; init; } = null!;
        public string Name { get; init; } = null!;
        public string AuthSendListen { get; init; } = null!;
        public string? AuthManage { get; init; }
    }
}
