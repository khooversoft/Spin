using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public class ArticleDirectoryBuilder
{
    public IList<ArticleManifest> Manifests { get; } = new List<ArticleManifest>();
    public ArticleDirectoryBuilder Add(ArticleManifest manifest) => this.Action(x => x.Manifests.Add(manifest.Verify()));

    public ArticleDirectoryBuilder Add(IEnumerable<ArticleManifest> manifests)
    {
        manifests.NotNull().ForEach(x => Manifests.Add(x.Verify()));
        return this;
    }

    public Option<GraphMap> Build()
    {
        if (!VerifyArticleIds(out var r1)) return r1.ToOptionStatus<GraphMap>();

        var map = new GraphMap();
        Manifests.SelectMany(GetNodes).ForEach(x => map.Nodes.Add(x, true).ThrowOnError());
        Manifests.SelectMany(GetEdges).ForEach(x => map.Edges.Add(x, true).ThrowOnError());

        return map;
    }

    private bool VerifyArticleIds(out Option result)
    {
        var fileIds = Manifests.Select(x => x.ArticleId).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (fileIds.Length != Manifests.Count)
        {
            string msg = fileIds.Aggregate("Duplicate 'Article Ids'" + Environment.NewLine, (a, x) => a += x + Environment.NewLine);
            result = StatusCode.Conflict;
            return false;
        }

        result = StatusCode.OK;
        return true;
    }

    public IReadOnlyList<GraphNode> GetNodes(ArticleManifest subject) => [GetNodeKey(subject), .. GetFileNodeKeys(subject), .. GetIndexNodes(subject)];

    public IReadOnlyList<GraphEdge> GetEdges(ArticleManifest subject) => [.. GetFileEdges(subject), .. GetIndexEdges(subject)];

    public GraphNode GetNodeKey(ArticleManifest subject)
    {
        subject.NotNull();

        string tags = new Tags(subject.Tags)
            .Set(NBlogConstants.OrderBy, subject.Index.ToString())
            .Set(NBlogConstants.KeyHashTag, subject.GetArticleIdHash())
            .ToString();

        return new GraphNode($"article:{subject.ArticleId}", tags);
    }

    public IReadOnlyList<GraphNode> GetFileNodeKeys(ArticleManifest subject) => subject.NotNull()
        .GetCommands()
        .Select(x => new GraphNode($"file:{x.FileId}", subject.Tags))
        .ToArray();

    public IReadOnlyList<GraphNode> GetIndexNodes(ArticleManifest subject)
    {
        subject.NotNull();

        var nodes = new Tags(subject.Tags)
            .Where(x => x.Value.IsNotEmpty())
            .Where(x => !x.Key.EqualsIgnoreCase(NBlogConstants.DbTag))
            .SelectMany(x => x.Value!.Split(',', StringSplitOptions.RemoveEmptyEntries), (o, i) => (o.Key, Value: i))
            .Select(x => new GraphNode($"tag:{x.Key}/{x.Value}", subject.Tags))
            .ToArray();

        return nodes;
    }

    public static IReadOnlyList<GraphEdge> GetFileEdges(ArticleManifest subject) => subject.NotNull()
        .GetCommands()
        .Select(x => new GraphEdge($"article:{subject.ArticleId}", $"file:{x.FileId}", edgeType: "file", x.Attributes.Join(';')))
        .ToArray();

    public IReadOnlyList<GraphEdge> GetIndexEdges(ArticleManifest subject)
    {
        subject.NotNull();

        var nodes = new Tags(subject.Tags)
            .Where(x => x.Value.IsNotEmpty())
            .Where(x => !x.Key.EqualsIgnoreCase(NBlogConstants.DbTag))
            .SelectMany(x => x.Value!.Split(',', StringSplitOptions.RemoveEmptyEntries), (o, i) => (o.Key, Value: i))
            .Select(x => new GraphEdge($"tag:{x.Key}/{x.Value}", $"article:{subject.ArticleId}", edgeType: "tagIndex", tags: "tagIndex"))
            .ToArray();

        return nodes;
    }
}
