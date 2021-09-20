using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spin.Common.Configuration.Model
{
    public record StorageModel
    {
        public string Channel { get; init; } = null!;

        public string AccountName { get; init; } = null!;

        public string ContainerName { get; init; } = null!;

        public string AccountKey { get; init; } = null!;
    }
}
