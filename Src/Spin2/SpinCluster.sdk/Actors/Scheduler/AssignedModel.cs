﻿using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Smartc;

[GenerateSerializer, Immutable]
public sealed record AssignedModel
{
    [Id(0)] public string AgentId { get; init; } = null!;
    [Id(1)] public DateTime Date { get; init; } = DateTime.UtcNow;
    [Id(2)] public TimeSpan TimeToLive { get; init; } = TimeSpan.FromMinutes(30);
    [Id(3)] public AssignedCompleted? AssignedCompleted { get; init; }

    public DateTime ValidTo => Date + TimeToLive;
    public bool IsAssignable() => DateTime.UtcNow < ValidTo;

    public static IValidator<AssignedModel> Validator { get; } = new Validator<AssignedModel>()
        .RuleFor(x => x.AgentId).ValidResourceId(ResourceType.System, "agent")
        .RuleFor(x => x.Date).ValidDateTime()
        .RuleFor(x => x.TimeToLive).Must(x => x.TotalMinutes > 0, x => $"{x} is not valid for time to live")
        .RuleFor(x => x.AssignedCompleted).ValidateOption(AssignedCompleted.Validator)
        .Build();
}

public static class AssignedModelExtensions
{
    public static Option Validate(this AssignedModel work) => AssignedModel.Validator.Validate(work).ToOptionStatus();
}
