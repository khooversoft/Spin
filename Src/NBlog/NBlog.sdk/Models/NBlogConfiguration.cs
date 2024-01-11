using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

[GenerateSerializer, Immutable]
public record NBlogConfiguration
{
    [Id(0)] public IReadOnlyList<IndexGroup> IndexGroups { get; init; } = Array.Empty<IndexGroup>();

    public static IValidator<NBlogConfiguration> Validator { get; } = new Validator<NBlogConfiguration>()
        .RuleForEach(x => x.IndexGroups).Validate(IndexGroup.Validator)
        .RuleForObject(x => x).Must(x =>
        {
            var names = x.IndexGroups
                .GroupBy(x => x.GroupName, StringComparer.OrdinalIgnoreCase)
                .Where(x => x.Count() != 1)
                .Select(x => x.Key)
                .ToArray();

            return names.Length == 0 ? StatusCode.OK : (StatusCode.BadRequest, $"Duplicate group names={names.Join(';')}");
        })
        .Build();
}


public static class NBlogConfigurationExtentions
{
    public static Option Validate(this NBlogConfiguration subject) => NBlogConfiguration.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this NBlogConfiguration subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
