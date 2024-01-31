using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

[GenerateSerializer, Immutable]
public record NBlogConfiguration
{
    [Id(1)] public string DbName { get; init; } = null!;
    [Id(2)] public string Theme { get; init; } = null!;
    [Id(3)] public string? HomeIndex { get; init; }
    [Id(4)] public OverviewPanel OverviewPanel { get; init; } = null!;
    [Id(5)] public IndexPanel IndexPanel { get; init; } = null!;
    [Id(6)] public IReadOnlyList<IndexGroup> IndexGroups { get; init; } = Array.Empty<IndexGroup>();

    public static IValidator<NBlogConfiguration> Validator { get; } = new Validator<NBlogConfiguration>()
        .RuleFor(x => x.DbName).ValidName()
        .RuleFor(x => x.Theme).NotEmpty().Must(NBlogConstants.ValidThemes.Contains, x => $"{x} not a valid theme")
        .RuleFor(x => x.OverviewPanel).Validate(OverviewPanel.Validator)
        .RuleFor(x => x.IndexPanel).Validate(IndexPanel.Validator)
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

    public static Option VerifyFileId(this NBlogConfiguration subject, string fileId, ScopeContext context)
    {
        string shouldBeActorKey = NBlogConstants.Tool.CreateConfigurationActorKey(subject.DbName);
        if (fileId != shouldBeActorKey)
        {
            context.LogError("NBlogConfiguration.DbName={dbName} does not match fileId={fileId}", subject.DbName, fileId);
            return (StatusCode.Conflict, "NBlogConfiguration.DbName does not match fileId");
        }

        return StatusCode.OK;
    }
}
