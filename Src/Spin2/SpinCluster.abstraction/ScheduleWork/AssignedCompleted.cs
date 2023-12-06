using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.abstraction;

[GenerateSerializer, Immutable]
public sealed record AssignedCompleted
{
    [Id(0)] public string AgentId { get; init; } = null!;
    [Id(1)] public string WorkId { get; init; } = null!;
    [Id(2)] public DateTime Date { get; init; } = DateTime.UtcNow;
    [Id(3)] public StatusCode StatusCode { get; init; }
    [Id(4)] public string Message { get; init; } = null!;

    public override string ToString() => $"AgentId={AgentId}, WorkId={WorkId}, Date={Date}, StatusCode={StatusCode}, Message={Message}";

    public static IValidator<AssignedCompleted> Validator { get; } = new Validator<AssignedCompleted>()
        .RuleFor(x => x.AgentId).ValidResourceId(ResourceType.System, "agent")
        .RuleFor(x => x.WorkId).NotEmpty()
        .RuleFor(x => x.Date).ValidDateTime()
        .RuleFor(x => x.StatusCode).ValidEnum()
        .RuleFor(x => x.Message).NotEmpty()
        .Build();
}

public static class AssignedCompletedExtensions
{
    public static Option Validate(this AssignedCompleted subject) => AssignedCompleted.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this AssignedCompleted subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}