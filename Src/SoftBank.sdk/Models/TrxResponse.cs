﻿using SoftBank.sdk.Models;
using Toolbox.Tools;
using Toolbox.Types;

namespace SoftBank.sdk.Models;

[GenerateSerializer, Immutable]
public sealed record TrxResponse
{
    [Id(0)] public string Id { get; init; } = Guid.NewGuid().ToString();
    [Id(1)] public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    [Id(2)] public TrxRequest Request { get; init; } = null!;
    [Id(3)] public TrxStatusCode Status { get; init; }
    [Id(4)] public decimal? Amount { get; set; }
    [Id(5)] public string? Error { get; init; }
    [Id(6)] public string? SourceLedgerItemId { get; init; } = null!;
    [Id(7)] public string DestinationLedgerItemId { get; init; } = null!;
}

public static class TrxResponseValidator
{
    public static IValidator<TrxResponse> Validator { get; } = new Validator<TrxResponse>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.Request).Validate(TrxRequest.Validator)
        .RuleFor(x => x.Status).ValidEnum()
        .RuleFor(x => x.DestinationLedgerItemId).NotEmpty()
        .Build();

    public static Option Validate(this TrxResponse request) => Validator.Validate(request).ToOptionStatus();
}
