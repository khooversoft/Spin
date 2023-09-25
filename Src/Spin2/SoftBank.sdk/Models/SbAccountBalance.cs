using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SoftBank.sdk.Models;

[GenerateSerializer, Immutable]
public sealed record SbAccountBalance
{
    [Id(0)] public string DocumentId { get; init; } = null!;
    [Id(1)] public decimal PrincipalBalance { get; init; }
}


public static class AccountBalanceValidator
{
    public static IValidator<SbAccountBalance> Validator { get; } = new Validator<SbAccountBalance>()
        .RuleFor(x => x.DocumentId).ValidAccountId()
        .Build();

    public static Option Validate(this SbAccountBalance subject) => Validator.Validate(subject).ToOptionStatus();
}