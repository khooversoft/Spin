using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public class ArticleService
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<ArticleService> _logger;
    private readonly ManifestService _manifestService;
    private readonly ArticleDirectoryClient _directory;

    public ArticleService(IClusterClient clusterClient, ArticleDirectoryClient directory, ManifestService manifestService, ILogger<ArticleService> logger)
    {
        _clusterClient = clusterClient.NotNull();
        _directory = directory.NotNull();
        _manifestService = manifestService.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<ArticleDetail>> ReadArticleDetail(string articleId, string attribute, ScopeContext context)
    {
        articleId.NotEmpty();
        attribute.NotEmpty();
        context = context.With(_logger);

        var manifestOption = await _manifestService.GetManifest(articleId, context);
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

    public Task<IReadOnlyList<ArticleReference>> GetSummaries(string dbName, ScopeContext context) => _directory.GetSummaries(dbName, context);

    public Task<IReadOnlyList<ArticleIndex>> GetIndexSummaries(string dbName, ScopeContext context) => _directory.GetSummaryIndexes(dbName, context);

    public Task<IReadOnlyList<ArticleIndex>> GetIndexSummariesByName(string dbName, string indexName, ScopeContext context) => _directory.GetSummaryIndexes(dbName, indexName, context);

    public Task<IReadOnlyList<ArticleIndex>> GetIndexDocs(ScopeContext context) => _directory.GetDocIndexes("article", context);

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

