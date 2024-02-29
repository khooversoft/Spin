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
    [Id(2)] public string Author { get; init; } = null!;
    [Id(3)] public int? Index { get; init; }
    [Id(4)] public DateTime CreatedDate { get; init; } = DateTime.UtcNow;
    [Id(5)] public DateTime? StartDate { get; init; }
    [Id(6)] public DateTime? EndDate { get; init; }
    [Id(7)] public bool NoShowDate { get; init; }
    [Id(8)] public bool NoTagLinks { get; init; }
    [Id(9)] public string? TitleLink { get; init; }
    [Id(10)] public string? LeftBoxStyle { get; init; }
    [Id(11)] public IReadOnlyList<string> Commands { get; init; } = Array.Empty<string>();
    [Id(12)] public string Tags { get; init; } = null!;

    public string GetArticleIdHash() => ArticleId.NotNull().ToBytes().ToHexHash();
    public static string CalculateArticleIdHash(string articleId) => articleId.NotNull().ToLower().ToBytes().ToHexHash();

    public static IValidator<ArticleManifest> Validator { get; } = new Validator<ArticleManifest>()
        .RuleFor(x => x.ArticleId).Must(x => FileId.Create(x).IsOk(), _ => "Invalid artical Id")
        .RuleFor(x => x.Title).NotEmpty()
        .RuleFor(x => x.Author).NotEmpty()
        .RuleFor(x => x.Tags).NotEmpty().Must(ArticleManifestValidations.RequiredTags)
        .RuleForObject(x => x).Must(x =>
        {
            bool status = ValidDateTimeTool.IsValidDateTime(x.CreatedDate) ||
                (ValidDateTimeTool.IsValidDateTime(x.StartDate) && ValidDateTimeTool.IsValidDateTime(x.EndDate));

            return status ? StatusCode.OK : (StatusCode.BadRequest, "Invalid date");
        })
        .RuleForObject(x => x).Must(ArticleManifestValidations.DistinctTests)
        .RuleForObject(x => x).Must(ArticleManifestValidations.RequiredAttributes)
        .Build();
}


public static class ArticleManifestValidations
{
    public static int GetIndexOrStartDate(this ArticleManifest subject) => subject.Index switch
    {
        int v => v,
        _ => subject.StartDate switch
        {
            DateTime v => DateTimeToReverseIndex(v),
            _ => DateTimeToReverseIndex(subject.CreatedDate),
        },
    };

    private static int DateTimeToReverseIndex(DateTime dateTime)
    {
        TimeSpan timeSpan = DateTime.MaxValue - dateTime;
        return (int)timeSpan.TotalDays;
    }

    public static Option DistinctTests(ArticleManifest manifest)
    {
        if (!GetCommands(manifest, out var commandsOption)) return commandsOption.ToOptionStatus();
        IReadOnlyList<CommandNode> commands = commandsOption.Return();

        int fileIdDistinctCount = commands.Select(x => x.FileId).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        if (fileIdDistinctCount != commands.Count) return (StatusCode.BadRequest, "Duplicate FileIds");

        int localFilePathCount = commands.Select(x => x.FileIdValue).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        if (localFilePathCount != commands.Count) return (StatusCode.BadRequest, "Duplicate LocalFilePath");

        return StatusCode.OK;
    }

    public static Option RequiredAttributes(ArticleManifest manifest)
    {
        if (!GetCommands(manifest, out var commandsOption)) return commandsOption.ToOptionStatus();
        IReadOnlyList<CommandNode> commands = commandsOption.Return();
        IReadOnlyList<string> shouldHave = [NBlogConstants.MainAttribute, NBlogConstants.SummaryAttribute];

        var attributes = commands.SelectMany(x => x.Attributes).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var contains = shouldHave.Where(x => attributes.Contains(x)).ToArray();

        return contains.Length != 0 ? StatusCode.OK : (StatusCode.Conflict, $"Missing one of the required attributes={shouldHave.Join(';')}");
    }

    public static Option RequiredTags(string tags)
    {
        var tokensOption = TagsTool.Parse(tags);
        if (tokensOption.IsError()) return tokensOption.ToOptionStatus();

        IReadOnlyList<KeyValuePair<string, string?>>? tokens = tokensOption.Return();

        var matched = NBlogConstants.RequiredTags
            .Select(x => tokens.TryGetValue(x, out var _) ? x : null)
            .OfType<string>();

        var except = NBlogConstants.RequiredTags.Except(matched).ToArray();

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
