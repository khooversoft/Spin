using Toolbox.Data;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.abstraction;

public record ScheduleOption
{
    public string SchedulerId { get; init; } = null!;
    public string PrincipalId { get; init; } = null!;
    public string SourceId { get; init; } = null!;
    public string AgentId { get; init; } = null!;

    public static IValidator<ScheduleOption> Validator { get; } = new Validator<ScheduleOption>()
        .RuleFor(x => x.SchedulerId).ValidResourceId(ResourceType.System, "scheduler")
        .RuleFor(x => x.PrincipalId).ValidResourceId(ResourceType.Principal)
        .RuleFor(x => x.SourceId).ValidName()
        .RuleFor(x => x.AgentId).ValidResourceId(ResourceType.System, "agent")
        .Build();
}


public static class ScheduleOptionExtensions
{
    public static Option Validate(this ScheduleOption subject) => ScheduleOption.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this ScheduleOption subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static ScheduleCreateModel CreateSchedule(this ScheduleOption subject, string command, string smartcId)
    {
        var set = new DataObjectSet();
        return CreateSchedule(subject, command, smartcId, set);
    }

    public static ScheduleCreateModel CreateSchedule<T1>(this ScheduleOption subject, string command, string smartcId, T1 data1) where T1 : class
    {
        var set = new DataObjectSet().Set(data1);
        return CreateSchedule(subject, command, smartcId, set);
    }

    public static ScheduleCreateModel CreateSchedule<T1, T2>(this ScheduleOption subject, string command, string smartcId, T1 data1, T2 data2) where T1 : class where T2 : class
    {
        var set = new DataObjectSet().Set(data1).Set(data2);
        return CreateSchedule(subject, command, smartcId, set);
    }

    public static ScheduleCreateModel CreateSchedule(this ScheduleOption option, string command, string smartcId, DataObjectSet dataSet)
    {
        option.NotNull().Validate().ThrowOnError();
        command.NotEmpty();
        smartcId.NotEmpty();
        dataSet.NotNull();

        var createRequest = new ScheduleCreateModel
        {
            SmartcId = smartcId,
            SchedulerId = option.SchedulerId,
            PrincipalId = option.PrincipalId,
            SourceId = option.SourceId,
            Command = command,
            Payloads = dataSet,
        };

        return createRequest;
    }
}
