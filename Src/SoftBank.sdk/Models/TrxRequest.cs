﻿using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SoftBank.sdk.Models;

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
    [Id(4)] public string AccountID { get; init; } = null!;
    [Id(5)] public string PartyAccountId { get; set; } = null!;
    [Id(6)] public string Description { get; init; } = null!;
    [Id(7)] public TrxType Type { get; init; }
    [Id(8)] public decimal Amount { get; set; }
    [Id(9)] public string? Tags { get; init; }

    public static IValidator<TrxRequest> Validator { get; } = new Validator<TrxRequest>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.AccountID).ValidAccountId()
        .RuleFor(x => x.PartyAccountId).ValidAccountId()
        .RuleFor(x => x.Description).NotEmpty()
        .RuleFor(x => x.Type).ValidEnum()
        .RuleFor(x => x.Amount).Must(x => x > 0.0m, x => $"Amount {x} is invalid enum")
        .Build();
}

public static class TrxRequestValidator
{
    public static Option Validate(this TrxRequest request) => TrxRequest.Validator.Validate(request).ToOptionStatus();

    public static bool Validate(this TrxRequest request, out Option result)
    {
        result = request.Validate();
        return result.IsOk();
    }
}
