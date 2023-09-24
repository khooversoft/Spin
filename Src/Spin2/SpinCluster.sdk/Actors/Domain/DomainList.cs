using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Domain;

[GenerateSerializer, Immutable]
public sealed record DomainList
{
    [Id(0)] public IReadOnlyList<string> ExternalDomains { get; init; } = Array.Empty<string>();

    public static IValidator<DomainList> Validator { get; } = new Validator<DomainList>()
        .RuleForEach(x => x.ExternalDomains).NotEmpty()
        .Build();
}


public static class DomainListExtensions
{
    public static Option Validate(this DomainList subject) => DomainList.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this DomainList subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}