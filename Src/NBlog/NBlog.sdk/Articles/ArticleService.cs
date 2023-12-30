using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Types;
using Toolbox.Tools;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;

namespace NBlog.sdk;

public class ArticleService
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<ArticleService> _logger;
    public ArticleService(IClusterClient clusterClient, ILogger<ArticleService> logger) => (_clusterClient, _logger) = (clusterClient.NotNull(), logger.NotNull());

    public async Task<Option<ArticleDetail>> ReadArticleDetail(string articleId, string attribute, ScopeContext context)
    {
        articleId.NotEmpty();
        attribute.NotEmpty();

        var manifestOption = await GetManifest(articleId, context);
        if (manifestOption.IsError()) return manifestOption.ToOptionStatus<ArticleDetail>();
        ArticleManifest manifest = manifestOption.Return();

        var commandNodeOption = GetCommandNode(manifest, attribute, context);
        if (commandNodeOption.IsError()) return commandNodeOption.ToOptionStatus<ArticleDetail>();

        CommandNode commandNode = commandNodeOption.Return();
        context.Location().LogInformation("Reading articleId={articleId}, fileId={fileId}", articleId, commandNode.FileId);
        var dataOption = await _clusterClient.GetStorageActor(commandNode.FileId).Get(context.TraceId);
        if (dataOption.IsError())
        {
            context.Location().LogError("Could not find articleId={articleId}, fileId={fileId}", articleId, commandNode.FileId);
            return (StatusCode.NotFound, "No fileId");
        }

        DataETag data = dataOption.Return();
        if (!data.Validate(out var v)) return v.LogOnError(context, "DataETag").ToOptionStatus<ArticleDetail>();

        return new ArticleDetail
        {
            Manifest = manifest,
            MarkdownDoc = new MarkdownDoc(data.Data),
        };
    }

    public Task<Option<IReadOnlyList<ArticleManifest>>> GetSummaries(ScopeContext context) => GetSummaries(x => !x.Tags.Has(NBlogConstants.NoSummaryTag), context);
    public Task<Option<IReadOnlyList<ArticleManifest>>> GetToolSummaries(ScopeContext context) => GetSummaries(x => x.Tags.Has(NBlogConstants.ToolTag), context);
    public Task<Option<IReadOnlyList<ArticleManifest>>> GetFrameworkSummaries(ScopeContext context) => GetSummaries(x => x.Tags.Has(NBlogConstants.FrameworkDesignTag), context);

    private async Task<Option<IReadOnlyList<ArticleManifest>>> GetSummaries(Func<GraphNode, bool> filter, ScopeContext context)
    {
        Option<GraphCommandResults> response = await _clusterClient.GetDirectoryActor().Execute("select (key=article:*);", context.TraceId);
        if (response.IsError()) return response.LogOnError(context, "Directory search failed").ToOptionStatus<IReadOnlyList<ArticleManifest>>();

        GraphCommandResults result = response.Return();
        IReadOnlyList<GraphNode> nodes = result.Items
            .SelectMany(x => x.Nodes())
            .Where(x => filter(x))
            .ToArray();

        var queue = new ConcurrentQueue<ArticleManifest>();
        await ActionBlockParallel.Run(getArticleDetail, nodes);

        var list = queue.OrderByDescending(x => x.CreatedDate).ToArray();
        return list;


        async Task getArticleDetail(GraphNode node)
        {
            string articleId = node.Key.Replace("article:", string.Empty);

            var manifestOption = await GetManifest(articleId, context);
            if (manifestOption.IsError()) return;
            ArticleManifest manifest = manifestOption.Return();

            queue.Enqueue(manifest);
        }
    }

    private async Task<Option<ArticleManifest>> GetManifest(string articleId, ScopeContext context)
    {
        string label = $"Article Id={articleId}";

        var readOption = await _clusterClient.GetArticleManifestActor(articleId).Get(context.TraceId);
        if (readOption.IsError()) return readOption.LogOnError(context, label);

        ArticleManifest manifest = readOption.Return();
        if (!manifest.Validate(out var v)) return v.LogOnError(context, label).ToOptionStatus<ArticleManifest>();

        return manifest;
    }

    private Option<CommandNode> GetCommandNode(ArticleManifest manifest, string attribute, ScopeContext context)
    {
        var command = manifest.GetCommand(attribute);
        if (command.IsError())
        {
            context.Location().LogError("Could not find attribute={attribute} in articleId={articleId}", manifest.ArticleId, attribute);
            return (StatusCode.NotFound, "No attribute");
        }

        return command;
    }
}

