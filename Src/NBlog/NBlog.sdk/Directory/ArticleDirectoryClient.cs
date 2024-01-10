using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public async Task<IReadOnlyList<ArticleFileReference>> GetSummaries(string dbName, ScopeContext context)
    {
        var query = $"select ('db={dbName.ToLower()}') a1 -> [summary;toKey=file:*];";

        var result = await _directoryActor.Execute(query, context.TraceId);
        if (result.IsError())
        {
            context.Location().LogError("Query failed to execute, query={query} with directoryActor", query);
            return Array.Empty<ArticleFileReference>();
        }

        GraphQueryResult graphResult = result.Return().Items[0];

        var nodes = graphResult.Alias["a1"].OfType<GraphNode>().ToArray();
        var edges = graphResult.Items.OfType<GraphEdge>().ToArray();

        var references = nodes
            .Join(edges,
                x => x.Key,
                x => x.FromKey,
                (o, i) => new ArticleFileReference(RemovePrefix(i.FromKey), o.Tags[NBlogConstants.CreatedDate].NotNull(), RemovePrefix(i.ToKey))
                )
            .ToArray();

        return references;
    }

    public Task<IReadOnlyList<ArticleIndex>> GetSummaryIndexes(string dbName, ScopeContext context) => GetIndexes(dbName, "*", "summary", context);
    public Task<IReadOnlyList<ArticleIndex>> GetSummaryIndexes(string dbName, string indexName, ScopeContext context) => GetIndexes(dbName, indexName, "summary", context);

    public Task<IReadOnlyList<ArticleIndex>> GetDocIndexes(string dbName, ScopeContext context) => GetIndexes(dbName, "*", "main", context);

    public async Task<IReadOnlyList<ArticleIndex>> GetIndexes(string dbName, string indexName, string docAttribute, ScopeContext context)
    {
        var query = $"select (key=tag:{indexName.ToLower()}) -> [tagIndex] a0 -> ('db={dbName.ToLower()}') -> [{docAttribute};toKey=file:*] a1;";

        var result = await _directoryActor.Execute(query, context.TraceId);
        if (result.IsError())
        {
            context.Location().LogError("Query failed to execute, query={query} with directoryActor", query);
            return Array.Empty<ArticleIndex>();
        }

        result.Return().Items.Assert(x => x.Count == 1, x => $"Invalid response, count={x.Count} != 1");
        GraphQueryResult graphResult = result.Return().Items[0];

        var indexSet = graphResult.Alias["a0"].OfType<GraphEdge>().ToArray();
        var edgeSet = graphResult.Alias["a1"].OfType<GraphEdge>().ToArray();

        var indexes = indexSet
            .Join(edgeSet, x => x.ToKey, x => x.FromKey, (o, i) => (tagNodeKey: o.FromKey, manifestNodeKey: i.FromKey, fileKey: i.ToKey))
            .Select(x => new ArticleIndex(RemovePrefix(x.tagNodeKey), RemovePrefix(x.manifestNodeKey), RemovePrefix(x.fileKey)))
            .ToArray();

        return indexes;
    }

    private static string RemovePrefix(string value) => _graphNamespace.Aggregate(value, (a, i) => a = value.StartsWith(i) ? value[i.Length..] : a);
}
