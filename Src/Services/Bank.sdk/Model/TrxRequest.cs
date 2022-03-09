using Bank.sdk.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Document;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Bank.sdk.Model;

/// <summary>
/// TrxTypes...
/// 
///   Credit - Push = (debit) FromId => (credit) ToId
///   Debit - Pull (credit) FromId => (debit) ToId
///   
/// Example:
///     Bank1 = 100.00
///     Bank2 = 200.00
///   
///     Trx = Amt=50.00, FromId=Bank1, ToId=Bank2
///     
///     Bank1 = -50.00 = 50.00
///     Bank2 = 50.00 = 250.00
///     
///     Response
///   
/// </summary>
public record TrxRequest
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public DateTime Date { get; init; } = DateTime.UtcNow;

    public TrxType Type { get; init; }

    public string FromId { get; init; } = null!;

    public string ToId { get; init; } = null!;

    public decimal Amount { get; init; }

    public IReadOnlyList<string> Properties { get; init; } = null!;
}


public static class TrxRequestExtensions
{
    public static (bool Pass, string? Message) IsVerify(this TrxRequest subject, string? fromBankName = null, string? toBankName = null)
    {
        subject.VerifyNotNull(nameof(subject));

        if (subject.Id.IsEmpty()) return (false, $"{nameof(subject.Id)} is required");
        if (subject.Type != TrxType.Credit && subject.Type != TrxType.Debit) return (false, $"Invalid type {subject.Type}");
        if (subject.FromId.IsEmpty()) return (false, $"{nameof(subject.FromId)} is required");
        if (subject.ToId.IsEmpty()) return (false, $"{nameof(subject.ToId)} is required");

        if (!isValidId(subject.FromId)) return (false, $"Invalid fromId={subject.FromId}");
        if (!isValidId(subject.ToId)) return (false, $"Invalid toId={subject.ToId}");

        return (true, null);

        static bool isValidId(string id) => id.IsDocumentIdValid().Valid && ((DocumentId)id).IsValidBankAccount();
    }

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