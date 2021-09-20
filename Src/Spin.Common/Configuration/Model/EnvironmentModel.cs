using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spin.Common.Configuration.Model
{
    public record EnvironmentModel
    {
        public IReadOnlyList<StorageModel> Storage { get; init; } = Array.Empty<StorageModel>();

        public IReadOnlyList<QueueModel> Queue { get; init; } = Array.Empty<QueueModel>();
    }
}
