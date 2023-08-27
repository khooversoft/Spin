using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SoftBank.sdk.Models;

public record AmountReserved
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string LeaseKey { get; init; } = null!;
    public string AccountId { get; init; } = null!;
    public string PrincipalId { get; init; } = null!;
    public decimal Amount { get; init; }
    public DateTime GoodTo { get; init; }
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