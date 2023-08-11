﻿using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Lease;

[GenerateSerializer, Immutable]
public record LeaseData
{
    [Id(0)] public string LeaseId { get; init; } = null!;
    [Id(1)] public string ObjectId { get; init; } = null!;
    [Id(2)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(3)] public TimeSpan TimeToLive { get; init; } = TimeSpan.FromSeconds(60);
}


public static class LeaseDataValidator
{
    public static Validator<LeaseData> Validator { get; } = new Validator<LeaseData>()
        .RuleFor(x => x.LeaseId).NotEmpty()
        .RuleFor(x => x.ObjectId).NotEmpty()
        .Build();

    public static ValidatorResult Validate(this LeaseData subject, ScopeContextLocation location) => Validator
        .Validate(subject)
        .LogResult(location);

    public static bool IsLeaseValid(this LeaseData data) => data != null && DateTime.UtcNow < data.CreatedDate + data.TimeToLive;
}