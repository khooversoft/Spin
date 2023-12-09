using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.abstraction;

public record ClientOption
{
    public string ClusterApiUri { get; init; } = null!;

    public static IValidator<ClientOption> Validator { get; } = new Validator<ClientOption>()
        .RuleFor(x => x.ClusterApiUri).NotEmpty()
        .Build();
}


public static class ClientOptionExtensions
{
    public static Option Validate(this ClientOption subject) => ClientOption.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this ClientOption subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
