using Toolbox.Extensions;
using Toolbox.Protocol;
using Toolbox.Tools;

namespace InstallmentContract.Provider.Models;

public record InitialDetail
{
    public DateTimeOffset Date { get; init; }
    public int NumPayments { get; init; }
    public decimal PrincipalAmount { get; init; }
    public decimal PaymentAmount { get; init; }
    public int FrequencyInDays { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset? CompletedDate { get; init; }
}


public static class InstallmentHeaderExtensions
{
    public static bool IsValid(this InitialDetail subject) =>
        subject != null &&
        subject.NumPayments >= 0 &&
        subject.PrincipalAmount >= 0.00m &&
        subject.PaymentAmount >= 0.00m &&
        subject.FrequencyInDays >= 0;
}
