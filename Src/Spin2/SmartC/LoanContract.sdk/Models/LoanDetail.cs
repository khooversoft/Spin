using LoanContract.sdk.Application;
using SoftBank.sdk.Application;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace LoanContract.sdk.Models;

public sealed record LoanDetail
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string ContractId { get; init; } = null!;
    public string OwnerId { get; init; } = null!;
    public string OwnerSoftBankId { get; init; } = null!;
    public DateTime FirstPaymentDate { get; init; }
    public decimal PrincipalAmount { get; init; }
    public decimal Payment { get; init; }
    public double APR { get; init; }
    public int NumberPayments { get; init; }
    public int PaymentsPerYear { get; init; }
    public string PartnerPrincipalId { get; init; } = null!;
    public string PartnerSoftBankId { get; init; } = null!;
    public DateTime CreatedDate { get; init; } = DateTime.UtcNow;

    public static IValidator<LoanDetail> Validator { get; } = new Validator<LoanDetail>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.ContractId).ValidResourceId(ResourceType.DomainOwned, LoanConstants.Schema)
        .RuleFor(x => x.OwnerId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.OwnerSoftBankId).ValidResourceId(ResourceType.DomainOwned, SoftBankConstants.Schema.SoftBankSchema)
        .RuleFor(x => x.FirstPaymentDate).ValidDateTime()
        .RuleFor(x => x.PrincipalAmount).Must(x => x > 0.0m, x => $"{x} must be greater then 0.0")
        .RuleFor(x => x.Payment).Must(x => x > 0.0m, x => $"{x} must be greater then 0.0")
        .RuleFor(x => x.APR).Must(x => x > 0.0, x => $"{x} must be greater then 0.0")
        .RuleFor(x => x.NumberPayments).Must(x => x > 0, x => $"{x} must be greater then 0")
        .RuleFor(x => x.PaymentsPerYear).Must(x => x > 0, x => $"{x} must be greater then 0")
        .RuleFor(x => x.PartnerPrincipalId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.PartnerSoftBankId).ValidResourceId(ResourceType.DomainOwned, SoftBankConstants.Schema.SoftBankSchema)
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .Build();
}

public static class LoanDetailModelExtensions
{
    public static Option Validate(this LoanDetail subject) => LoanDetail.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this LoanDetail subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}

