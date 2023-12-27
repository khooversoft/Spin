using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public static class ArticleManifestExtensions
{
    public static Option Validate(this ArticleManifest subject) => ArticleManifest.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this ArticleManifest subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static IReadOnlyList<CommandNode> GetCommands(this ArticleManifest subject) => subject.Commands.SelectMany(x => CommandGrammarParser.Parse(x).Return()).ToArray();

    public static Option<CommandNode> GetCommand(this ArticleManifest subject, string attribute) => subject.GetCommands()
        .Where(x => x.Attributes.Any(y => y == attribute))
        .FirstOrDefaultOption();

    public static IReadOnlyList<GraphNode> GetNodes(this ArticleManifest subject) => [subject.GetNodeKey(), .. subject.GetFileNodeKeys(), .. subject.GetCategoryNodeKeys()];

    public static IReadOnlyList<GraphEdge> GetEdges(this ArticleManifest subject) => [.. subject.GetFileEdges(), .. subject.GetCategoryEdges()];

    public static GraphNode GetNodeKey(this ArticleManifest subject)
    {
        string tags = new Tags(subject.Tags).SetValue(NBlogConstants.CreatedDate, subject.CreatedDate.ToString("o")).ToString();
        return new GraphNode($"article:{subject.NotNull().ArticleId}", tags);
    }

    public static IReadOnlyList<GraphNode> GetFileNodeKeys(this ArticleManifest subject) => subject.NotNull()
        .GetCommands()
        .Select(x => new GraphNode($"file:{x.FileId}", subject.Tags))
        .ToArray();

    public static IReadOnlyList<GraphNode> GetCategoryNodeKeys(this ArticleManifest subject) => subject.NotNull().Category switch
    {
        null => Array.Empty<GraphNode>(),

        string v => v.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => new GraphNode($"category:{x}"))
            .ToArray(),
    };

    public static IReadOnlyList<GraphEdge> GetFileEdges(this ArticleManifest subject) => subject.NotNull()
        .GetCommands()
        .Select(x => new GraphEdge($"article:{subject.ArticleId}", $"file:{x.FileId}", "file", subject.Tags))
        .ToArray();

    public static IReadOnlyList<GraphEdge> GetCategoryEdges(this ArticleManifest subject) => subject.NotNull().Category switch
    {
        null => Array.Empty<GraphEdge>(),

        string v => v.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => new GraphEdge($"category:{x}", $"article:{subject.ArticleId}", "category", subject.Tags))
            .ToArray(),
    };
}