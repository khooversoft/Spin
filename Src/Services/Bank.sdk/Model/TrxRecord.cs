using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Bank.sdk.Model;

[DebuggerDisplay("Type={Type}, Amount={Amount}")]
public record TrxRecord
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public DateTime Date { get; init; } = DateTime.UtcNow;

    public TrxType Type { get; init; }

    public decimal Amount { get; init; }

    public IReadOnlyList<string>? Properties { get; init; }

    public string? TrxRequestId { get; init; }
}


public static class TrxRecordExtensions
{
    public static decimal NaturalAmount(this TrxRecord trxRecord) => trxRecord
        .VerifyNotNull(nameof(TrxRequest))
        .Func(x => x.Type switch
        {
            TrxType.Credit => x.Amount,
            TrxType.Debit => -x.Amount,

            _ => throw new ArgumentException($"Unknown type={x.Type}"),
        });

    public static decimal Balance(this IEnumerable<TrxRecord> trxRecords) => trxRecords.Sum(x => x.NaturalAmount());
}
