using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Smart_Installment.sdk;

public record InstallmentHeader
{
    public string Issuer { get; init; } = null!;
    public string Description { get; init; } = null!;
    public int NumPayments { get; init; }
    public decimal Principal { get; init; }
    public decimal Payment { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset? CompletedDate { get; init; }
}


public static class InstallmentHeaderExtensions
{
    public static InstallmentHeader Verify(this InstallmentHeader subject)
    {
        subject.NotNull();

        subject.Issuer.NotEmpty();
        subject.Description.NotEmpty();
        subject.NumPayments.Assert(x => x > 0, "NumPayment must be > 0");
        subject.Principal.Assert(x => x > 0.00m, "Principal amount must be > 0");
        subject.Payment.Assert(x => x > 0.00m, "Payment amount must be > 0");

        return subject;
    }
}