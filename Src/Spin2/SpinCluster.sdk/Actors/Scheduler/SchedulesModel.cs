using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Smartc;

[GenerateSerializer, Immutable]
public sealed record SchedulesModel
{
    [Id(0)] public List<ScheduleWorkModel> WorkItems { get; init; } = new List<ScheduleWorkModel>();
    [Id(1)] public List<ScheduleWorkModel> CompletedItems { get; init; } = new List<ScheduleWorkModel>();

    public static IValidator<SchedulesModel> Validator { get; } = new Validator<SchedulesModel>()
        .RuleForEach(x => x.WorkItems).Validate(ScheduleWorkModel.Validator)
        .RuleForEach(x => x.CompletedItems).Validate(ScheduleWorkModel.Validator)
        .Build();
}

public static class ScheduleWorkExtensions
{
    public static Option Validate(this SchedulesModel work) => SchedulesModel.Validator.Validate(work).ToOptionStatus();
}
