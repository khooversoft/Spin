﻿using Toolbox.Extensions;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SoftBank.sdk;

public enum LedgerType
{
    Credit,
    Debit
}

//[GenerateSerializer, Immutable]
public record LedgerItem
{
    /*[Id(0)]*/ public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    /*[Id(1)]*/ public string Id { get; init; } = Guid.NewGuid().ToString();
    /*[Id(2)]*/ public required string Description { get; init; } = null!;
    /*[Id(3)]*/ public required LedgerType Type { get; init; }
    /*[Id(4)]*/ public required decimal Amount { get; init; }

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
