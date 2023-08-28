using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SoftBank.sdk.Models;

[GenerateSerializer, Immutable]
public record AmountReserved
{
    [Id(0)] public string Id { get; init; } = Guid.NewGuid().ToString();
    [Id(1)] public string LeaseKey { get; init; } = null!;
    [Id(2)] public string AccountId { get; init; } = null!;
    [Id(3)] public string PrincipalId { get; init; } = null!;
    [Id(4)] public decimal Amount { get; init; }
    [Id(5)] public DateTime GoodTo { get; init; }
}


public static class AmountReservedValidator
{
    public static IValidator<AmountReserved> Validator { get; } = new Validator<AmountReserved>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.LeaseKey).NotEmpty()
        .RuleFor(x => x.AccountId).ValidAccountId()
        .RuleFor(x => x.PrincipalId).ValidPrincipalId()
        .RuleFor(x => x.Amount).Must(x => x > 0, _ => "Amount must be greater then zero")
        .RuleFor(x => x.GoodTo).ValidDateTime()
        .Build();

    public static Option Validate(this AmountReserved subject) => Validator.Validate(subject).ToOptionStatus();
}