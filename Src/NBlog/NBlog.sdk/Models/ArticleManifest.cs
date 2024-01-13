using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

[GenerateSerializer, Immutable]
public record ArticleManifest
{
    [Id(0)] public string ArticleId { get; init; } = null!;
    [Id(1)] public string Title { get; init; } = null!;
    [Id(3)] public string Author { get; init; } = null!;
    [Id(4)] public int? Index { get; init; }
    [Id(5)] public DateTime CreatedDate { get; init; }
    [Id(6)] public DateTime? StartDate { get; init; }
    [Id(7)] public DateTime? EndDate { get; init; }
    [Id(8)] public IReadOnlyList<string> Commands { get; init; } = Array.Empty<string>();
    [Id(9)] public string Tags { get; init; } = null!;

    public static IValidator<ArticleManifest> Validator { get; } = new Validator<ArticleManifest>()
        .RuleFor(x => x.ArticleId).Must(x => FileId.Create(x).IsOk(), _ => "Invalid artical Id")
        .RuleFor(x => x.Title).NotEmpty()
        .RuleFor(x => x.Author).NotEmpty()
        .RuleFor(x => x.CreatedDate).ValidDateTime()
        .RuleFor(x => x.Tags).NotEmpty().Must(ArticleManifestValidations.RequiredTags)
        .RuleForObject(x => x).Must(ArticleManifestValidations.DistinctTests)
        .RuleForObject(x => x).Must(ArticleManifestValidations.RequiredAttributes)
        .Build();
}


public static class ArticleManifestValidations
{
    public static Option DistinctTests(ArticleManifest manifest)
    {
        if (!GetCommands(manifest, out var commandsOption)) return commandsOption.ToOptionStatus();
        IReadOnlyList<CommandNode> commands = commandsOption.Return();

        int fileIdDistinctCount = commands.Select(x => x.FileId).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        if (fileIdDistinctCount != commands.Count) return (StatusCode.BadRequest, "Duplicate FileIds");

        int localFilePathCount = commands.Select(x => x.LocalFilePath).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        if (localFilePathCount != commands.Count) return (StatusCode.BadRequest, "Duplicate LocalFilePath");

        return StatusCode.OK;
    }

    public static Option RequiredAttributes(ArticleManifest manifest)
    {
        if (!GetCommands(manifest, out var commandsOption)) return commandsOption.ToOptionStatus();
        IReadOnlyList<CommandNode> commands = commandsOption.Return();

        IReadOnlyList<string> shouldHave = TagsTool.HasTag(manifest.Tags, NBlogConstants.NoSummaryTag) switch
        {
            true => [NBlogConstants.MainAttribute],
            false => [NBlogConstants.MainAttribute, NBlogConstants.SummaryAttribute],
        };

        var attributes = commands.SelectMany(x => x.Attributes).ToArray();
        var contains = shouldHave.Where(x => attributes.Contains(x)).ToArray();
        var missing = shouldHave.Except(contains).ToArray();

        return missing.Length == 0 ? StatusCode.OK : (StatusCode.Conflict, $"Missing attributes={missing.Join(';')}");
    }

    public static Option RequiredTags(string tags)
    {
        IReadOnlyList<string> requiredTags = [NBlogConstants.DbTag, NBlogConstants.AreaTag];

        var matched = requiredTags
            .Select(x => TagsTool.TryGetValue(tags, x, out var _) ? x : null)
            .OfType<string>();

        var except = requiredTags.Except(matched).ToArray();

        return except.Length == 0 ? StatusCode.OK : (StatusCode.BadRequest, $"Missing required tags={except.Join(';')}");
    }

    private static bool GetCommands(ArticleManifest manifest, out Option<IReadOnlyList<CommandNode>> commandNodes)
    {
        var parseResultOption = manifest.Commands.Select(x => CommandGrammarParser.Parse(x)).ToArray();
        var errors = parseResultOption.Where(x => x.IsError()).Select(x => $"StatusCode={x.StatusCode}, Error={x.Error}").Join(';');
        if (errors.IsNotEmpty())
        {
            commandNodes = (StatusCode.BadRequest, errors);
            return false;
        }

        commandNodes = parseResultOption.SelectMany(x => x.Return()).ToArray();
        return true;
    }
}
