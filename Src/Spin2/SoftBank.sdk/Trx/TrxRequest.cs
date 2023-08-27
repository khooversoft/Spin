﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SoftBank.sdk.Trx;

public enum TrxType
{
    Push = 1,
    Pull = 2,
}

// Push = source - debig, destination - credit
// Pull = source - credit, destination - debit

public enum TrxStatusCode
{
    Completed = 200,

    BadRequest = 400,
    Forbidden = 403,
    NotFound = 404,

    NoFunds = 1000,
}

[GenerateSerializer, Immutable]
public sealed record TrxRequest
{
    [Id(0)] public string Id { get; init; } = Guid.NewGuid().ToString();
    [Id(1)] public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    [Id(2)] public string PrincipalId { get; init; } = null!;
    [Id(3)] public string SourceAccountID { get; init; } = null!;
    [Id(4)] public string DestinationAccountId { get; set; } = null!;
    [Id(5)] public string Description { get; init; } = null!;
    [Id(6)] public TrxType Type { get; init; }
    [Id(7)] public decimal Amount { get; set; }
}

public static class TrxRequestValidator
{
    public static IValidator<TrxRequest> Validator { get; } = new Validator<TrxRequest>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.PrincipalId).ValidPrincipalId()
        .RuleFor(x => x.SourceAccountID).ValidAccountId()
        .RuleFor(x => x.DestinationAccountId).ValidAccountId()
        .RuleFor(x => x.Description).NotEmpty()
        .RuleFor(x => x.Type).Must(x => x.IsEnumValid(), x => $"Enum {x.ToString()} is invalid enum")
        .RuleFor(x => x.Amount).Must(x => x > 0.0m, x => $"Amount {x} is invalid enum")
        .Build();

    public static Option Validate(this TrxRequest request) => Validator.Validate(request).ToOptionStatus();
}
