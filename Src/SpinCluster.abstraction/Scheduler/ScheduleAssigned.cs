using SpinCluster.abstraction;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.abstraction;

public record ScheduleAssigned
{
    public ScheduleOption ScheduleOption { get; init; } = null!;
    public WorkAssignedModel WorkAssigned { get; init; } = null!;

    public static IValidator<ScheduleAssigned> Validator { get; } = new Validator<ScheduleAssigned>()
        .RuleFor(x => x.ScheduleOption).Validate(ScheduleOption.Validator)
        .RuleFor(x => x.WorkAssigned).Validate(WorkAssignedModel.Validator)
        .Build();
}


public static class ScheduleAssignedExtensions
{
    public static Option Validate(this ScheduleAssigned subject) => ScheduleAssigned.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this ScheduleAssigned subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}