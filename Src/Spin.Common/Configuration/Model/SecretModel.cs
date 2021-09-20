using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Spin.Common.Configuration.Model
{
    public record SecretModel
    {
        public IReadOnlyDictionary<string, string> Data { get; init; } = null!;
    }
}
