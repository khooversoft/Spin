﻿using System.Diagnostics;
using SpinCluster.abstraction;
using Toolbox.Tools;
using Toolbox.Types;

namespace LoanContract.sdk.Models;

public enum LoanLedgerType
{
    Credit = 1,
    Debit = 2,
}

public enum LoanTrxType
{
    Payment = 1,
    InterestCharge = 2,
    PrincipalCharge = 3,
}

[DebuggerDisplay("Type={Type}, TrxType={TrxType}, Amount={Amount}, Description={Description}")]
public record LoanLedgerItem
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public DateTime PostedDate { get; init; } = DateTime.UtcNow;
    public string ContractId { get; init; } = null!;
    public string OwnerId { get; init; } = null!;
    public string Description { get; init; } = null!;
    public LoanLedgerType Type { get; init; }
    public LoanTrxType TrxType { get; init; }
    public decimal Amount { get; init; }
    public string? Tags { get; init; }
    public DateTime CreateDate { get; init; } = DateTime.UtcNow;

    public decimal NaturalAmount => Type.NaturalAmount(Amount);

    public static IValidator<LoanLedgerItem> Validator { get; } = new Validator<LoanLedgerItem>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.PostedDate).ValidDateTime()
        .RuleFor(x => x.ContractId).ValidResourceId(ResourceType.DomainOwned, SpinConstants.Schema.Contract)
        .RuleFor(x => x.OwnerId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.Description).NotEmpty()
        .RuleFor(x => x.Type).ValidEnum()
        .RuleFor(x => x.TrxType).ValidEnum()
        .RuleFor(x => x.Amount).Must(x => x >= 0, x => $"{x} must be greater then or equal 0")
        .RuleFor(x => x.CreateDate).ValidDateTime()
        .Build();
}


public static class LedgerItemModelExtensions
{
    public static Option Validate(this LoanLedgerItem subject) => LoanLedgerItem.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this LoanLedgerItem subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static decimal NaturalAmount(this LoanLedgerType type, decimal amount) => type switch
    {
        LoanLedgerType.Credit => Math.Abs(amount),
        LoanLedgerType.Debit => -Math.Abs(amount),

        _ => throw new ArgumentException($"Invalid type={type}")
    };

    public static decimal NaturalAmount(this LoanLedgerItem subject) => subject.Type.NaturalAmount(subject.Amount);
}
