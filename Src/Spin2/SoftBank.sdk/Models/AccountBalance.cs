using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SoftBank.sdk.Models;

[GenerateSerializer, Immutable]
public sealed record AccountBalance
{
    [Id(0)] public string DocumentId { get; init; } = null!;
    [Id(1)] public decimal Balance { get; init; }
    [Id(2)] public decimal LedgerBalance { get; init; }
}


public static class AccountBalanceValidator
{
    public static IValidator<AccountBalance> Validator { get; } = new Validator<AccountBalance>()
        .RuleFor(x => x.DocumentId).ValidAccountId()
        .Build();

    public static Option Validate(this AccountBalance subject) => Validator.Validate(subject).ToOptionStatus();
}