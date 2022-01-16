using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.sdk.Models;

public record BlkHeader : BlkBase
{
    public string Owner { get; init; } = null!;

    public string Description { get; init; } = null!;

    public DateTime Created { get; init; } = DateTime.UtcNow;
}
