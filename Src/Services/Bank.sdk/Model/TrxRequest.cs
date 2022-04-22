using Bank.sdk.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
///     Bank1 => -50.00 = 50.00
///     Bank2 => 50.00 = 250.00
///     
///     Response
///   
/// </summary>
[DebuggerDisplay("FromId={FromId}, ToId={ToId}, Amount={Amount}")]
public record TrxRequest
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public DateTime Date { get; init; } = DateTime.UtcNow;

    public string FromId { get; init; } = null!;

    public string ToId { get; init; } = null!;

    public decimal Amount { get; init; }

    public IReadOnlyList<string>? Properties { get; init; }
}


public static class TrxRequestExtensions
{
    public static bool IsVerify(this TrxRequest trxRequest)
    {
        if (trxRequest == null) return false;

        if (!isValid(trxRequest.FromId)) return false;
        if (!isValid(trxRequest.ToId)) return false;

        return true;

        static bool isValid(string subject) =>
            !subject.IsEmpty() &&
            subject.IsDocumentIdValid().Valid &&
            subject.ToDocumentId().ToBankAccountId() != null;
    }
}
