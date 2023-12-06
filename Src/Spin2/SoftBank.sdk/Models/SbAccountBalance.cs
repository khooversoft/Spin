using Toolbox.Tools;
using Toolbox.Types;

namespace SoftBank.sdk.Models;

[GenerateSerializer, Immutable]
public sealed record SbAccountBalance
{
    [Id(0)] public string DocumentId { get; init; } = null!;
    [Id(1)] public decimal PrincipalBalance { get; init; }
    [Id(2)] public decimal ReserveBalance { get; init; }
}


public static class AccountBalanceValidator
{
    public static IValidator<SbAccountBalance> Validator { get; } = new Validator<SbAccountBalance>()
        .RuleFor(x => x.DocumentId).ValidAccountId()
        .RuleFor(x => x.PrincipalBalance).Must(x => x > 0, x => $"{x} is invalid")
        .RuleFor(x => x.ReserveBalance).Must(x => x > 0, x => $"{x} is invalid")
        .Build();

    public static Option Validate(this SbAccountBalance subject) => Validator.Validate(subject).ToOptionStatus();
}