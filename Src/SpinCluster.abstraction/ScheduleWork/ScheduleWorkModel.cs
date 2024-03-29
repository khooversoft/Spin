﻿using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.abstraction;

// Command Type = "args", "Json:{type}"
[GenerateSerializer, Immutable]
public sealed record ScheduleWorkModel
{
    [Id(0)] public string WorkId { get; init; } = null!;
    [Id(1)] public string SchedulerId { get; init; } = null!;
    [Id(2)] public string SmartcId { get; init; } = null!;
    [Id(3)] public DateTime CreateDate { get; init; } = DateTime.UtcNow;
    [Id(4)] public DateTime? ValidTo { get; init; }
    [Id(5)] public DateTime? ExecuteAfter { get; init; }
    [Id(6)] public string SourceId { get; init; } = null!;
    [Id(7)] public string Command { get; init; } = null!;
    [Id(8)] public AssignedModel? Assigned { get; init; }
    [Id(9)] public DataObjectSet Payloads { get; init; } = new DataObjectSet();
    [Id(10)] public IReadOnlyList<RunResultModel> RunResults { get; init; } = Array.Empty<RunResultModel>();
    [Id(11)] public string? Tags { get; init; }

    public static IValidator<ScheduleWorkModel> Validator { get; } = new Validator<ScheduleWorkModel>()
        .RuleFor(x => x.WorkId).ValidResourceId(ResourceType.System, SpinConstants.Schema.ScheduleWork)
        .RuleFor(x => x.SchedulerId).ValidResourceId(ResourceType.System, SpinConstants.Schema.Scheduler)
        .RuleFor(x => x.SmartcId).ValidResourceId(ResourceType.DomainOwned, "smartc")
        .RuleFor(x => x.SourceId).ValidName()
        .RuleFor(x => x.Command).NotEmpty()
        .RuleFor(x => x.Assigned).ValidateOption(AssignedModel.Validator)
        .RuleFor(x => x.Payloads).NotNull().Validate(DataObjectSet.Validator)
        .RuleForEach(x => x.RunResults).Validate(RunResultModel.Validator)
        .Build();
}


public static class ScheduleWorkModelExtensions
{
    public static Option Validate(this ScheduleWorkModel subject) => ScheduleWorkModel.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this ScheduleWorkModel subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static ScheduleWorkState GetState(this ScheduleWorkModel subject) => subject.NotNull() switch
    {
        { Assigned: not null } v => v.Assigned.GetState(),
        _ => ScheduleWorkState.Available,
    };
}