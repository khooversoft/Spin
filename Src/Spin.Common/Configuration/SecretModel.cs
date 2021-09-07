using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spin.Common.Configuration
{
    public record SecretModel
    {
        public IReadOnlyDictionary<string, string> Data { get; init; } = null!;
    }
}
