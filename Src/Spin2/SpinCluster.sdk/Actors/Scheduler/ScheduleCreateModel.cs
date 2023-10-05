using SpinCluster.sdk.Application;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.ScheduleWork;

[GenerateSerializer, Immutable]
public sealed record ScheduleCreateModel
{
    [Id(0)] public string WorkId { get; init; } = $"{SpinConstants.Schema.ScheduleWork}:WKID-{Guid.NewGuid()}";
    [Id(1)] public string SmartcId { get; init; } = null!;
    [Id(2)] public string PrincipalId { get; init; } = null!;
    [Id(3)] public DateTime? ValidTo { get; init; }
    [Id(4)] public DateTime? ExecuteAfter { get; init; }
    [Id(5)] public string SourceId { get; init; } = null!;
    [Id(6)] public string CommandType { get; init; } = "args";
    [Id(7)] public string Command { get; init; } = null!;
    [Id(8)] public DataObjectSet Payloads { get; init; } = new DataObjectSet();

    public static IValidator<ScheduleCreateModel> Validator { get; } = new Validator<ScheduleCreateModel>()
        .RuleFor(x => x.WorkId).ValidResourceId(ResourceType.System, SpinConstants.Schema.ScheduleWork)
        .RuleFor(x => x.SmartcId).ValidResourceId(ResourceType.DomainOwned, "smartc")
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.SourceId).ValidName()
        .RuleFor(x => x.CommandType).Must(x => x == "args" || x.StartsWith("json:"), x => $"{x} is not valid, must be 'args' or 'json:{{type}}'")
        .RuleFor(x => x.Command).NotEmpty()
        .RuleFor(x => x.Payloads).NotNull()
        .RuleFor(x => x.Payloads).Validate(DataObjectSet.Validator)
        .Build();
}

public static class ScheduleCreateModelExtensions
{
    public static Option Validate(this ScheduleCreateModel subject) => ScheduleCreateModel.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this ScheduleCreateModel subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static ScheduleWorkModel ConvertTo(this ScheduleCreateModel subject) => new ScheduleWorkModel
    {
        WorkId = subject.WorkId,
        SmartcId = subject.SmartcId,
        ValidTo = subject.ValidTo,
        ExecuteAfter = subject.ExecuteAfter,
        SourceId = subject.SourceId,
        CommandType = subject.CommandType,
        Command = subject.Command,
        Payloads = new DataObjectSet(subject.Payloads),
    };
}
