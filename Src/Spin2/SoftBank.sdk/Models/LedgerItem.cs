using Toolbox.Extensions;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SoftBank.sdk.Models;

public enum LedgerType
{
    Credit = 1,
    Debit = 2,
}

[GenerateSerializer, Immutable]
public record LedgerItem
{
    [Id(0)] public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    [Id(1)] public string Id { get; init; } = Guid.NewGuid().ToString();
    [Id(2)] public string OwnerId { get; init; } = null!;
    [Id(3)] public string Description { get; init; } = null!;
    [Id(4)] public LedgerType Type { get; init; }
    [Id(5)] public decimal Amount { get; init; }
    [Id(6)] public string? Tags { get; init; }

    public decimal NaturalAmount => Type.NaturalAmount(Amount);
}

public static class LedgerItemValidator
{
    public static IValidator<LedgerItem> Validator { get; } = new Validator<LedgerItem>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.OwnerId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.Description).NotEmpty()
        .RuleFor(x => x.Type).Must(x => x.IsEnumValid(), x => $"Enum {x.ToString()} is invalid enum")
        .RuleFor(x => x.Amount).Must(x => x >= 0, x => $"{x} must be greater then or equal 0")
        .Build();

    public static Option Validate(this LedgerItem subject) => Validator.Validate(subject).ToOptionStatus();

    public static decimal NaturalAmount(this LedgerType type, decimal amount) => type switch
    {
        LedgerType.Credit => Math.Abs(amount),
        LedgerType.Debit => -Math.Abs(amount),

        _ => throw new ArgumentException($"Invalid type={type}")
    };

    public static decimal GetNaturalAmount(this LedgerItem subject) => subject.Type.NaturalAmount(subject.Amount);
}
