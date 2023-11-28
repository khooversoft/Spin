using SpinCluster.abstraction;
using SpinCluster.sdk.Application;
using Toolbox.Data;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Scheduler;

public sealed record ScheduleCommandModel
{
    public string WorkId { get; init; } = $"{SpinConstants.Schema.ScheduleWork}:WKID-{Guid.NewGuid()}";
    public string SchedulerId { get; init; } = null!;
    public string SmartcId { get; init; } = null!;
    public string PrincipalId { get; init; } = null!;
    public DateTime? ValidTo { get; init; }
    public DateTime? ExecuteAfter { get; init; }
    public string SourceId { get; init; } = null!;
    public string Command { get; init; } = null!;
    public IReadOnlyDictionary<string, object>? Payloads { get; init; }
    public string? Tags { get; init; }


    public static IValidator<ScheduleCommandModel> Validator { get; } = new Validator<ScheduleCommandModel>()
        .RuleFor(x => x.WorkId).ValidResourceId(ResourceType.System, SpinConstants.Schema.ScheduleWork)
        .RuleFor(x => x.SchedulerId).ValidResourceId(ResourceType.System, SpinConstants.Schema.Scheduler)
        .RuleFor(x => x.SmartcId).ValidResourceId(ResourceType.DomainOwned, SpinConstants.Schema.Smartc)
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.SourceId).ValidName()
        .RuleFor(x => x.Command).NotEmpty()
        .RuleFor(x => x.Payloads).NotNull()
        .Build();
}

public static class ScheduleCommandModelExtensions
{
    public static Option Validate(this ScheduleCommandModel subject) => ScheduleCommandModel.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this ScheduleCommandModel subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static ScheduleCreateModel ConvertTo(this ScheduleCommandModel subject) => new ScheduleCreateModel
    {
        WorkId = subject.WorkId,
        SchedulerId = subject.SchedulerId,
        SmartcId = subject.SmartcId,
        PrincipalId = subject.PrincipalId,
        ValidTo = subject.ValidTo,
        ExecuteAfter = subject.ExecuteAfter,
        SourceId = subject.SourceId,
        Command = subject.Command,
        Payloads = new DataObjectSet(subject.Payloads),
        Tags = subject.Tags,
    };
}