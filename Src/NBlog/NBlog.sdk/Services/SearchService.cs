using Microsoft.Extensions.Logging;
using Toolbox.DocumentSearch;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public class SearchService
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<SearchService> _logger;
    private readonly ManifestService _manifestService;

    public SearchService(IClusterClient clusterClient, ManifestService manifestService, ILogger<SearchService> logger)
    {
        _clusterClient = clusterClient.NotNull();
        _manifestService = manifestService.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<IReadOnlyList<ArticleManifest>> Search(string dbName, string search, ScopeContext context)
    {
        context = context.With(_logger);
        ISearchActor searchActor = _clusterClient.GetSearchActor();

        Option<IReadOnlyList<DocumentReference>> resultOption = await searchActor.Search(search, context.TraceId);
        if (resultOption.IsError()) return Array.Empty<ArticleManifest>();
        IReadOnlyList<DocumentReference> result = resultOption.Return();

        var articleManifests = await _manifestService.GetManifests(result.Select(x => x.DocumentId).ToArray(), context);
        return articleManifests;
    }
}
