using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterApi.test.Application;

public record TestOption
{
    public string ClusterApiUri { get; init; } = null!;
    public string SchedulerId { get; init; } = null!;
}


public static class TestOptionExtensions
{
    public static IValidator<TestOption> Validator { get; } = new Validator<TestOption>()
        .RuleFor(x => x.ClusterApiUri).NotEmpty()
        .RuleFor(x => x.SchedulerId).ValidResourceId(ResourceType.System, "scheduler")
        .Build();

    public static Option Validate(this TestOption subject) => Validator.Validate(subject).ToOptionStatus();
}
