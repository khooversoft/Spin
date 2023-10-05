using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Actors.ScheduleWork;
using SpinCluster.sdk.Application;
using Toolbox.Data;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Scheduler;

public sealed record ScheduleCommandModel
{
    public string WorkId { get; init; } = $"{SpinConstants.Schema.ScheduleWork}:WKID-{Guid.NewGuid()}";
    public string SmartcId { get; init; } = null!;
    public string PrincipalId { get; init; } = null!;
    public DateTime? ValidTo { get; init; }
    public DateTime? ExecuteAfter { get; init; }
    public string SourceId { get; init; } = null!;
    public string CommandType { get; init; } = "args";
    public string Command { get; init; } = null!;
    public IReadOnlyDictionary<string, object>? Payloads { get; init; }


    public static IValidator<ScheduleCommandModel> Validator { get; } = new Validator<ScheduleCommandModel>()
        .RuleFor(x => x.WorkId).ValidResourceId(ResourceType.System, SpinConstants.Schema.ScheduleWork)
        .RuleFor(x => x.SmartcId).ValidResourceId(ResourceType.DomainOwned, "smartc")
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.SourceId).ValidName()
        .RuleFor(x => x.CommandType).Must(x => x == "args" || x.StartsWith("json:"), x => $"{x} is not valid, must be 'args' or 'json:{{type}}'")
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
        SmartcId = subject.SmartcId,
        PrincipalId = subject.PrincipalId,
        ValidTo = subject.ValidTo,
        ExecuteAfter = subject.ExecuteAfter,
        SourceId = subject.SourceId,
        CommandType = subject.CommandType,
        Command = subject.Command,
        Payloads = new DataObjectSet(subject.Payloads),
    };
}