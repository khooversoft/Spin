using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Spin.Common.Configuration
{
    public record EnviromentConfigModel
    {
        public IReadOnlyList<StorageModel>? Storages { get; init; }

        public IReadOnlyList<QueueModel>? Queue { get; init; }
    }
}
