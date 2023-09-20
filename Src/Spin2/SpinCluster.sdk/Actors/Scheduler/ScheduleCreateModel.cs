using SpinCluster.sdk.Actors.Smartc;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Scheduler;

[GenerateSerializer, Immutable]
public sealed record ScheduleCreateModel
{
    [Id(0)] public string WorkId { get; init; } = Guid.NewGuid().ToString();
    [Id(1)] public string SmartcId { get; init; } = null!;
    [Id(2)] public string PrincipalId { get; init; } = null!;
    [Id(3)] public DateTime? ValidTo { get; init; }
    [Id(4)] public DateTime? ExecuteAfter { get; init; }
    [Id(5)] public string SourceId { get; init; } = null!;
    [Id(6)] public string CommandType { get; init; } = "args";
    [Id(7)] public string Command { get; init; } = null!;

    public static IValidator<ScheduleCreateModel> Validator { get; } = new Validator<ScheduleCreateModel>()
    .RuleFor(x => x.WorkId).NotEmpty()
    .RuleFor(x => x.SmartcId).ValidResourceId(ResourceType.DomainOwned, "smartc")
    .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
    .RuleFor(x => x.SourceId).ValidName()
    .RuleFor(x => x.CommandType).Must(x => x == "args" || x.StartsWith("json:"), x => $"{x} is not valid, must be 'args' or 'json:{{type}}'")
    .RuleFor(x => x.Command).NotEmpty()
    .Build();
}

public static class ScheduleCreateModelExtensions
{
    public static Option Validate(this ScheduleCreateModel subject) => ScheduleCreateModel.Validator.Validate(subject).ToOptionStatus();

    public static ScheduleWorkModel ConvertTo(this ScheduleCreateModel subject) => new ScheduleWorkModel
    {
        WorkId = subject.WorkId,
        SmartcId = subject.SmartcId,
        ValidTo = subject.ValidTo,
        ExecuteAfter = subject.ExecuteAfter,
        SourceId = subject.SourceId,
        CommandType = subject.CommandType,
        Command = subject.Command,
    };
}
