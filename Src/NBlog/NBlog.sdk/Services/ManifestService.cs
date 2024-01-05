using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public class ManifestService
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<ManifestService> _logger;
    public ManifestService(IClusterClient clusterClient, ILogger<ManifestService> logger) => (_clusterClient, _logger) = (clusterClient.NotNull(), logger.NotNull());

    public async Task<Option<IReadOnlyList<ArticleManifest>>> GetManifests(IReadOnlyList<string> nodes, ScopeContext context)
    {
        context = context.With(_logger);

        var queue = new ConcurrentQueue<ArticleManifest>();
        await ActionBlockParallel.Run(getArticleDetail, nodes);

        var list = queue.OrderByDescending(x => x.CreatedDate).ToArray();
        return list;

        async Task getArticleDetail(string articleId)
        {
            articleId = articleId.Replace("article:", string.Empty);

            var manifestOption = await GetManifest(articleId, context);
            if (manifestOption.IsError()) return;
            ArticleManifest manifest = manifestOption.Return();

            queue.Enqueue(manifest);
        }
    }

    private async Task<Option<ArticleManifest>> GetManifest(string articleId, ScopeContext context)
    {
        context = context.With(_logger);

        string label = $"Article Id={articleId}";

        var readOption = await _clusterClient.GetArticleManifestActor(articleId).Get(context.TraceId);
        if (readOption.IsError()) return readOption.LogOnError(context, label);

        ArticleManifest manifest = readOption.Return();
        if (!manifest.Validate(out var v)) return v.LogOnError(context, label).ToOptionStatus<ArticleManifest>();

        return manifest;
    }
}
