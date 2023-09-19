using System;
using SpinCluster.sdk.Actors.Scheduler;
using Toolbox.Extensions;
using Toolbox.Tools;
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
    public static Option Validate(this SchedulesModel subject) => SchedulesModel.Validator.Validate(subject).ToOptionStatus();

    public static Option CompleteWork(this SchedulesModel subject, AssignedCompleted assignedCompleted, ScopeContext context)
    {
        subject.NotNull();
        assignedCompleted.NotNull();

        var v = assignedCompleted.Validate();
        if (v.IsError()) return v;

        var findOption = subject.FindWorkId(assignedCompleted.WorkId);
        if (findOption.IsError())
        {
            context.Location().LogError("Cannot find workId={workId} in active workitems to complete", assignedCompleted.WorkId);
            return findOption.ToOptionStatus();
        }

        int index = findOption.Return();
        ScheduleWorkModel removed = subject.WorkItems[index];
        subject.WorkItems.RemoveAt(index);

        removed = removed with
        {
            Assigned = removed.Assigned.NotNull() with { AssignedCompleted = assignedCompleted },
        };

        return StatusCode.OK;
    }

    public static Option AddRunResult(this SchedulesModel subject, RunResultModel runResult, ScopeContext context)
    {
        subject.NotNull();
        runResult.NotNull();

        var v = runResult.Validate();
        if (v.IsError()) return v;

        var findOption = subject.FindWorkId(runResult.WorkId);
        if (findOption.IsError())
        {
            context.Location().LogError("Cannot find workId={workId} in active workitems to add run result", runResult.WorkId);
            return findOption.ToOptionStatus();
        }

        int index = findOption.Return();

        subject.WorkItems[index] = subject.WorkItems[index] with
        {
            RunResults = subject.WorkItems[index].RunResults
                .Append(runResult)
                .ToArray(),
        };

        return StatusCode.OK;
    }

    public static Option<int> FindWorkId(this SchedulesModel subject, string workId)
    {
        subject.NotNull();

        int index = subject.WorkItems
            .WithIndex()
            .Where(x => x.Item.WorkId == workId)
            .Select(x => x.Index)
            .FirstOrDefault(-1);

        return index switch
        {
            -1 => new Option<int>(StatusCode.NotFound, $"FindWorkId: workId={workId} does not exist"),
            int v => v
        };
    }
}
