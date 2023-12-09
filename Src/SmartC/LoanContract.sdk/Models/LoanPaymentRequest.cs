using SpinCluster.abstraction;
using Toolbox.Tools;
using Toolbox.Types;

namespace LoanContract.sdk.Models;

public sealed record LoanPaymentRequest
{
    public string ContractId { get; init; } = null!;
    public string PrincipalId { get; init; } = null!;
    public DateTime PostedDate { get; init; } = DateTime.UtcNow;
    public decimal PaymentAmount { get; init; }
    public string? Tags { get; init; }

    public static IValidator<LoanPaymentRequest> Validator { get; } = new Validator<LoanPaymentRequest>()
        .RuleFor(x => x.ContractId).ValidResourceId(ResourceType.DomainOwned, SpinConstants.Schema.Contract)
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.PostedDate).ValidDateTime()
        .RuleFor(x => x.PaymentAmount).Must(x => x >= 0.0m, x => $"{x} is invalid")
        .Build();
}


public static class PaymentRequestModelExtensions
{
    public static Option Validate(this LoanPaymentRequest subject) => LoanPaymentRequest.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this LoanPaymentRequest subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
