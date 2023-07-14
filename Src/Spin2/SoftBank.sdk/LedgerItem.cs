using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SoftBank.sdk;

public enum LedgerType
{
    Credit,
    Debit
}

public record LedgerItem
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public required string Description { get; init; } = null!;
    public required LedgerType Type { get; init; }
    public required decimal Amount { get; init; }

    public decimal NaturalAmount => Type.NaturalAmount(Amount);
}

public static class LedgerTypeValidator
{
    public static IValidator<LedgerItem> Validator { get; } = new Validator<LedgerItem>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.Description).NotEmpty()
        .RuleFor(x => x.Type).Must(x => x.IsEnumValid<LedgerType>(), x => $"Enum {x.ToString()} is invalid enum")
        .RuleFor(x => x.Amount).Must(x => x >= 0, x => $"{x} must be greater then or equal 0")
        .Build();

    public static ValidatorResult Validate(this LedgerItem subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);

    public static bool IsValid(this LedgerItem subject, ScopeContextLocation location) => Validate(subject, location).IsValid;

    public static decimal NaturalAmount(this LedgerType type, decimal amount) => type switch
    {
        LedgerType.Credit => Math.Abs(amount),
        LedgerType.Debit => -Math.Abs(amount),

        _ => throw new ArgumentException($"Invalid type={type}")
    };
}
