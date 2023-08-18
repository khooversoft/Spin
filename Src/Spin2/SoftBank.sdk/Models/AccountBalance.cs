using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SoftBank.sdk.Models;

[GenerateSerializer, Immutable]
public sealed record AccountBalance
{
    [Id(0)] public string ObjectId { get; init; } = null!;
    [Id(1)] public string OwnerId { get; init; } = null!;
    [Id(2)] public string Name { get; init; } = null!;
    [Id(3)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(4)] public decimal Balance { get; init; }
}


public static class AccountBalanceValidator
{
    public static IValidator<AccountBalance> Validator { get; } = new Validator<AccountBalance>()
        .RuleFor(x => x.ObjectId).ValidObjectId()
        .RuleFor(x => x.OwnerId).ValidPrincipalId()
        .RuleFor(x => x.Name).NotEmpty()
        .Build();

    public static Option Validate(this AccountBalance subject) => Validator.Validate(subject).ToOptionStatus();
}