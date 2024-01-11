using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public class ArticleDirectoryClient
{
    private static IReadOnlyList<string> _graphNamespace = ["article:", "file:", "tag:"];
    private readonly IDirectoryActor _directoryActor;
    private readonly ILogger<ArticleDirectoryClient> _logger;

    public ArticleDirectoryClient(IDirectoryActor directoryActor, ILogger<ArticleDirectoryClient> logger)
    {
        _directoryActor = directoryActor.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<IReadOnlyList<ArticleReference>> GetSummaries(string dbName, ScopeContext context)
    {
        var query = $"select ('db={dbName.ToLower()}') a1 -> [summary;toKey=file:*];";

        var result = await _directoryActor.Execute(query, context.TraceId);
        if (result.IsError())
        {
            context.Location().LogError("Query failed to execute, query={query} with directoryActor", query);
            return Array.Empty<ArticleReference>();
        }

        GraphQueryResult graphResult = result.Return().Items[0];

        var nodes = graphResult.Alias["a1"].OfType<GraphNode>().ToArray();
        var edges = graphResult.Items.OfType<GraphEdge>().ToArray();

        var references = nodes
            .Join(edges,
                x => x.Key,
                x => x.FromKey,
                (o, i) => new ArticleReference(RemovePrefix(i.FromKey), o.Tags[NBlogConstants.CreatedDate].NotNull())
                )
            .ToArray();

        return references;
    }

    public Task<IReadOnlyList<ArticleIndex>> GetSummaryIndexes(string dbName, ScopeContext context) => GetIndexes(dbName, "*", "summary", context);
    public Task<IReadOnlyList<ArticleIndex>> GetSummaryIndexes(string dbName, string indexName, ScopeContext context) => GetIndexes(dbName, indexName, "summary", context);

    public Task<IReadOnlyList<ArticleIndex>> GetDocIndexes(string dbName, ScopeContext context) => GetIndexes(dbName, "*", "main", context);

    public async Task<IReadOnlyList<ArticleIndex>> GetIndexes(string dbName, string indexName, string docAttribute, ScopeContext context)
    {
        var query = $"select (key=tag:{indexName.ToLower()}) -> [tagIndex] a0 -> ('db={dbName.ToLower()}') a1 -> [{docAttribute};toKey=file:*] a2;";

        var result = await _directoryActor.Execute(query, context.TraceId);
        if (result.IsError())
        {
            context.Location().LogError("Query failed to execute, query={query} with directoryActor", query);
            return Array.Empty<ArticleIndex>();
        }

        result.Return().Items.Assert(x => x.Count == 1, x => $"Invalid response, count={x.Count} != 1");
        GraphQueryResult graphResult = result.Return().Items[0];

        var indexSet = graphResult.Alias["a0"].OfType<GraphEdge>().ToArray();
        var articleSet = graphResult.Alias["a1"].OfType<GraphNode>().ToArray();
        var edgeSet = graphResult.Alias["a2"].OfType<GraphEdge>().ToArray();

        var indexes = indexSet
            .Join(articleSet, x => x.ToKey, x => x.Key, (o, i) => (tagNodeKey: o.FromKey, articleKey: i.Key, articleTags: i.Tags))
            .Join(edgeSet, x => x.articleKey, x => x.FromKey, (o, i) => (o.tagNodeKey, o.articleKey, o.articleTags))
            .Select(x => new ArticleIndex
            {
                IndexName = RemovePrefix(x.tagNodeKey),
                ArticleId = RemovePrefix(x.articleKey),
                Title = x.articleTags[NBlogConstants.ArticleTitle].NotNull(),
                CreatedDate = DateTime.Parse(x.articleTags[NBlogConstants.CreatedDate].NotNull()),
            })
            .ToArray();

        return indexes;
    }

    private static string RemovePrefix(string value) => _graphNamespace.Aggregate(value, (a, i) => a = value.StartsWith(i) ? value[i.Length..] : a);
}
