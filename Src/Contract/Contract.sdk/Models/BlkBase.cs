using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.sdk.Models;

public record BlkBase
{
    public DateTime BlockDate { get; init; } = DateTime.UtcNow;

    public IReadOnlyDictionary<string, string> Properties { get; init; } = null!;
}
