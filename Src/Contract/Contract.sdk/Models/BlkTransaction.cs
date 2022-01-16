using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.sdk.Models;

public record BlkTransaction : BlkBase
{
    public IReadOnlyList<BlkRecord> Records { get; init; } = new List<BlkRecord>();
}

public record BlkRecord : BlkBase
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTime TrxDate { get; init; } = DateTime.UtcNow;

    public string TrxType { get; init; } = null!;

    public double Value { get; init; }

    public string? Note { get; init; }
}
