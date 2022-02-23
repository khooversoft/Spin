using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Bank.sdk.Model;

public record TrxRequest
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public string AccountId { get; init; } = null!;

    public DateTime Date { get; init; } = DateTime.UtcNow;

    public TrxType Type { get; init; }

    public decimal Amount { get; init; }

    public IReadOnlyList<string> Properties { get; init; } = null!;
}


public static class TrxRequestExtensions
{
    public static decimal NaturalAmount(this TrxRequest trxRecord) => trxRecord
        .VerifyNotNull(nameof(TrxRequest))
        .Func(x => x.Type switch
        {
            TrxType.Credit => x.Amount,
            TrxType.Debit => -x.Amount,

            _ => throw new ArgumentException($"Unknown type={x.Type}"),
        });

    public static decimal Balance(this IEnumerable<TrxRequest> trxRequests) => trxRequests
        .ToSafe()
        .Sum(x => x.NaturalAmount());
}