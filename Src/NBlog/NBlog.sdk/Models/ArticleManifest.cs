using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

[GenerateSerializer, Immutable]
public class ArticleManifest
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
        .RuleForEach(x => x.Commands).Must(x => CommandGrammarParser.Parse(x).IsOk(), x => $"{x} is invalid")
        .Build();
}


public static class ArticleManifestExtensions
{
    public static Option Validate(this ArticleManifest subject) => ArticleManifest.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this ArticleManifest subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static IReadOnlyList<CommandNode> GetCommands(this ArticleManifest subject) => subject.Commands.SelectMany(x => CommandGrammarParser.Parse(x).Return()).ToArray();
}