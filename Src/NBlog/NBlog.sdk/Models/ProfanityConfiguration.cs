using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public record ProfanityConfiguration
{
    public IReadOnlyList<string> Words { get; init; } = Array.Empty<string>();

    public static IValidator<ProfanityConfiguration> Validator { get; } = new Validator<ProfanityConfiguration>()
        .RuleForEach(x => x.Words).NotEmpty()
        .Build();
}

public static class ProfanityConfigurationExtensions
{
    public static Option Validate(this ProfanityConfiguration subject) => ProfanityConfiguration.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this ProfanityConfiguration subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}