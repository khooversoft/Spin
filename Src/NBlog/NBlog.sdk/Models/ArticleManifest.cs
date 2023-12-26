using Toolbox.Data;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

[GenerateSerializer, Immutable]
public record ArticleManifest
{
    [Id(0)] public string ArticleId { get; init; } = null!;
    [Id(1)] public string Title { get; init; } = null!;
    [Id(3)] public string Author { get; init; } = null!;
    [Id(5)] public DateTime CreatedDate { get; init; } = DateTime.Now;
    [Id(6)] public IReadOnlyList<string> Commands { get; init; } = Array.Empty<string>();
    [Id(7)] public string? Tags { get; init; }
    [Id(8)] public string? Category { get; init; }

    public static IValidator<ArticleManifest> Validator { get; } = new Validator<ArticleManifest>()
        .RuleFor(x => x.ArticleId).Must(x => FileId.Create(x).IsOk(), _ => "Invalid artical Id")
        .RuleFor(x => x.Title).NotEmpty()
        .RuleFor(x => x.Author).NotEmpty()
        .RuleForEach(x => x.Commands).Must(x =>
        {
            var commandsOption = CommandGrammarParser.Parse(x);
            if (commandsOption.IsError()) return commandsOption.ToOptionStatus<string>();
            var commands = commandsOption.Return();

            int fileIdDistinctCount = commands.Select(x => x.FileId).Distinct(StringComparer.OrdinalIgnoreCase).Count();
            if (fileIdDistinctCount != commands.Count) return (StatusCode.BadRequest, "Duplicate FileIds");

            int localFilePathCount = commands.Select(x => x.LocalFilePath).Distinct(StringComparer.OrdinalIgnoreCase).Count();
            if (localFilePathCount != commands.Count) return (StatusCode.BadRequest, "Duplicate LocalFilePath");

            return StatusCode.OK;
        })
        .Build();
}
