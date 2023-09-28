using SoftBank.sdk.Application;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SoftBank.sdk.Models;

public enum SbLedgerType
{
    Credit = 1,
    Debit = 2,
}

[GenerateSerializer, Immutable]
public record SbLedgerItem
{
    [Id(0)] public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    [Id(1)] public string Id { get; init; } = Guid.NewGuid().ToString();
    [Id(2)] public string AccountId { get; init; } = null!;
    [Id(3)] public string? PartyAccountId { get; init; }
    [Id(4)] public string OwnerId { get; init; } = null!;
    [Id(5)] public string Description { get; init; } = null!;
    [Id(6)] public SbLedgerType Type { get; init; }
    [Id(7)] public decimal Amount { get; init; }
    [Id(8)] public string? Tags { get; init; }

    public decimal NaturalAmount => Type.NaturalAmount(Amount);

    public static IValidator<SbLedgerItem> Validator { get; } = new Validator<SbLedgerItem>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.AccountId).ValidResourceId(ResourceType.DomainOwned, SoftBankConstants.Schema.SoftBankSchema)
        .RuleFor(x => x.OwnerId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.PartyAccountId).ValidResourceIdOption(ResourceType.DomainOwned, SoftBankConstants.Schema.SoftBankSchema)
        .RuleFor(x => x.Description).NotEmpty()
        .RuleFor(x => x.Type).ValidEnum()
        .RuleFor(x => x.Amount).Must(x => x >= 0, x => $"{x} must be greater then or equal 0")
        .Build();
}

public static class LedgerItemValidator
{
    public static Option Validate(this SbLedgerItem subject) => SbLedgerItem.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this SbLedgerItem subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static decimal NaturalAmount(this SbLedgerType type, decimal amount) => type switch
    {
        SbLedgerType.Credit => Math.Abs(amount),
        SbLedgerType.Debit => -Math.Abs(amount),

        _ => throw new ArgumentException($"Invalid type={type}")
    };

    public static decimal GetNaturalAmount(this SbLedgerItem subject) => subject.Type.NaturalAmount(subject.Amount);
}
