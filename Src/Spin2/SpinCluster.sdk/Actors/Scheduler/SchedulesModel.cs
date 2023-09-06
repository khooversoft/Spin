using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Tools.Validation;
using Toolbox.Extensions;

namespace SpinCluster.sdk.Actors.Scheduler;

[GenerateSerializer, Immutable]
public sealed record SchedulesModel
{
    [Id(0)] public Queue<ScheduleWork> WorkItems { get; } = new Queue<ScheduleWork>();

    public static IValidator<SchedulesModel> Validator { get; } = new Validator<SchedulesModel>()
        .RuleForEach(x => x.WorkItems).Validate(ScheduleWork.Validator)
        .Build();
}


// Command Type = "tags", "Json:{type}"
[GenerateSerializer, Immutable]
public sealed record ScheduleWork
{
    [Id(0)] public string Id { get; init; } = Guid.NewGuid().ToString();
    [Id(1)] public string AccountId { get; init; } = null!;
    [Id(2)] public DateTime CreateDate { get; init; } = DateTime.UtcNow;
    [Id(3)] public DateTime? ValidTo { get; init; }
    [Id(4)] public string SourceId { get; init; } = null!;
    [Id(5)] public string CommandType { get; init; } = null!;
    [Id(6)] public string Command { get; init; } = null!;
    [Id(7)] public Assigned? Assigned { get; init; }

    public static IValidator<ScheduleWork> Validator { get; } = new Validator<ScheduleWork>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.AccountId).ValidAccountId()
        .RuleFor(x => x.SourceId).ValidName()
        .RuleFor(x => x.CommandType).ValidName()
        .RuleFor(x => x.Command).NotEmpty()
        .RuleFor(x => x.Assigned).Must(
            x => x == null || Assigned.Validator.Validate(x).IsOk(),
            x => Assigned.Validator.Validate(x.NotNull()).Error ?? "< no error >"
            )
        .Build();
}

[GenerateSerializer, Immutable]
public sealed record Assigned
{
    [Id(0)] public string AgentId { get; init; } = null!;
    [Id(1)] public DateTime Date { get; init; } = DateTime.UtcNow;
    [Id(2)] public TimeSpan TimeToLive { get; init; } = TimeSpan.FromMinutes(2);

    public DateTime ValidTo => Date + TimeToLive;
    public bool IsValid => DateTime.UtcNow < ValidTo;

    public static IValidator<Assigned> Validator { get; } = new Validator<Assigned>()
        .RuleFor(x => x.AgentId).ValidName()
        .Build();
}

public static class ScheduleWorkExtensions
{
    public static Option Validate(this SchedulesModel work) => SchedulesModel.Validator.Validate(work).ToOptionStatus();
    public static Option Validate(this ScheduleWork work) => ScheduleWork.Validator.Validate(work).ToOptionStatus();
    public static Option Validate(this Assigned work) => Assigned.Validator.Validate(work).ToOptionStatus();
}
