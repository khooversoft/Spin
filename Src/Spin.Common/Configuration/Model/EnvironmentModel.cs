using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spin.Common.Configuration.Model
{
    public record EnvironmentModel
    {
        public List<StorageModel> Storage { get; init; } = new List<StorageModel>();

        public List<QueueModel> Queue { get; init; } = new List<QueueModel>();
    }
}
