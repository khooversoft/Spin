using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.sdk.Model;

public record TrxRecord
{
    public DateTime Date { get; init; } = DateTime.UtcNow;

    public TrxType Type { get; init; }

    public string? Memo { get; init; }

    public decimal Amount { get; init; }
}
