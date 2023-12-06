using Toolbox.Tools;
using Toolbox.Types;

namespace SoftBank.sdk.Models;

[GenerateSerializer, Immutable]
public record SbAmountReserved
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
    public static IValidator<SbAmountReserved> Validator { get; } = new Validator<SbAmountReserved>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.LeaseKey).NotEmpty()
        .RuleFor(x => x.AccountId).ValidAccountId()
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.Amount).Must(x => x > 0, _ => "Amount must be greater then zero")
        .RuleFor(x => x.GoodTo).ValidDateTime()
        .Build();

    public static Option Validate(this SbAmountReserved subject) => Validator.Validate(subject).ToOptionStatus();
}