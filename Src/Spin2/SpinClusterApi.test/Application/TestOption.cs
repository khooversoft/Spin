using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinClusterApi.test.Application;

public record TestOption
{
    public string ClusterApiUri { get; init; } = null!;
}


public static class TestOptionExtensions
{
    public static IValidator<TestOption> Validator { get; } = new Validator<TestOption>()
        .RuleFor(x => x.ClusterApiUri).NotEmpty()
        .Build();

    public static Option Validate(this TestOption subject) => Validator.Validate(subject).ToOptionStatus();

    public static TestOption Verify(this TestOption subject)
    {
        Validator.Validate(subject).ThrowOnError();
        return subject;
    }
}
