using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Abstractions.Model;

[DebuggerDisplay("FromId={Reference.FromId}, ToId={Reference.ToId}, Amount={Reference.Amount}, Status={Status}")]
public record TrxRequestResponse
{
    public string Id { get; init; } = Guid.NewGuid().ToString();

    public TrxRequest Reference { get; init; } = null!;

    public DateTime Date { get; init; } = DateTime.UtcNow;

    public TrxStatus Status { get; init; } = TrxStatus.Unknown;
}
