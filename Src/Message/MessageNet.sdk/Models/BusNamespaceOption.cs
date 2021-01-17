using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageNet.sdk.Models
{
    public record BusNamespaceOption
    {
        public string Namespace { get; init; } = null!;

        public string BusNamespace { get; init; } = null!;

        public string KeyName { get; init; } = null!;

        public string AccessKey { get; init; } = null!;
    }
}
