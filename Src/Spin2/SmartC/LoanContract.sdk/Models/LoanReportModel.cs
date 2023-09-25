namespace LoanContract.sdk.Models;

public sealed record LoanReportModel
{
    public string ContractId { get; init; } = null!;
    public LoanDetail LoanDetail { get; init; } = null!;
    public IReadOnlyList<LoanLedgerItem> LedgerItems { get; init; } = null!;

    public decimal GetPrincipalAmount() => LoanDetail.PrincipalAmount - LedgerItems
        .Select(x => x.NaturalAmount())
        .Sum(x => x);
}
