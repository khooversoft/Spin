using System.Diagnostics;

namespace LoanContract.sdk.Models;

public sealed record LoanReportModel
{
    public string ContractId { get; init; } = null!;
    public LoanDetail LoanDetail { get; init; } = null!;
    public IReadOnlyList<LoanLedgerItem> LedgerItems { get; init; } = null!;
    public IReadOnlyList<LedgerBalanceItem> BalanceItems { get; init; } = null!;
}


[DebuggerDisplay("PostedDate={PostedDate}, CreditCharge={CreditCharge}, Payment={Payment}, ToPrincipal={ToPrincipal}, PrincipalBalance={PrincipalBalance}")]
public sealed record LedgerBalanceItem
{
    public DateTime PostedDate { get; init; } = DateTime.UtcNow;
    public decimal CreditCharge { get; init; }
    public decimal Payment { get; init; }
    public decimal ToPrincipal { get; init; }
    public decimal PrincipalBalance { get; init; }
}
