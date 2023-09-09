using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Smartc;

// Command Type = "args", "Json:{type}"
[GenerateSerializer, Immutable]
public sealed record ScheduleWorkModel
{
    [Id(0)] public string WorkId { get; init; } = Guid.NewGuid().ToString();
    [Id(1)] public string SmartcId { get; init; } = null!;
    [Id(2)] public DateTime CreateDate { get; init; } = DateTime.UtcNow;
    [Id(3)] public DateTime? ValidTo { get; init; }
    [Id(4)] public DateTime? ExecuteAfter { get; init; }
    [Id(5)] public string SourceId { get; init; } = null!;
    [Id(6)] public string CommandType { get; init; } = "args";
    [Id(7)] public string Command { get; init; } = null!;
    [Id(8)] public AssignedModel? Assigned { get; init; }
    [Id(9)] public RunResultModel? RunResult { get; init; }

    public static IValidator<ScheduleWorkModel> Validator { get; } = new Validator<ScheduleWorkModel>()
        .RuleFor(x => x.WorkId).NotEmpty()
        .RuleFor(x => x.SmartcId).ValidResourceId(ResourceType.DomainOwned, "smartc")
        .RuleFor(x => x.SourceId).ValidName()
        .RuleFor(x => x.CommandType).Must(x => x == "args" || x.StartsWith("json:"), x => $"{x} is not valid, must be 'args' or 'json:{{type}}'")
        .RuleFor(x => x.Command).NotEmpty()
        .RuleFor(x => x.Assigned).Must(
            x => x == null || AssignedModel.Validator.Validate(x).IsOk(),
            x => AssignedModel.Validator.Validate(x.NotNull()).Error ?? "< no error >"
            )
        .RuleFor(x => x.RunResult).Must(
            x => x == null || RunResultModel.Validator.Validate(x).IsOk(),
            x => RunResultModel.Validator.Validate(x.NotNull()).Error ?? "< no error >"
            )
        .Build();
}


public static class ScheduleWorkModelExtensions
{
    public static Option Validate(this ScheduleWorkModel work) => ScheduleWorkModel.Validator.Validate(work).ToOptionStatus();
}