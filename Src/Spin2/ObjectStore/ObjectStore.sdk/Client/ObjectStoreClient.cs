using Azure;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.DataLake;
using Toolbox.Data;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace ObjectStore.sdk.Client;

public class ObjectStoreClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ObjectStoreClient> _logger;

    public ObjectStoreClient(HttpClient httpClient, ILogger<ObjectStoreClient> logger)
    {
        _httpClient = httpClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<Document>> Read(ObjectId objectId, ScopeContext context) => await new RestClient(_httpClient)
        .SetPath($"data/{objectId.ToUrlEncoding()}")
        .GetAsync(context.With(_logger))
        .GetContent<Document>();

    public async Task<Option<ETag>> Write(Document document, ScopeContext context) => await new RestClient(_httpClient)
        .SetPath("data")
        .SetContent(document)
        .PostAsync(context.With(_logger))
        .GetContent<ETag>();

    public async Task<StatusCode> Delete(ObjectId objectId, ScopeContext context) => await new RestClient(_httpClient)
        .SetPath($"data/{objectId.ToUrlEncoding()}")
        .DeleteAsync(context.With(_logger))
        .GetStatusCode();

    public async Task<Option<BatchQuerySet<DatalakePathItem>>> Search(QueryParameter queryParameter, ScopeContext context) => await new RestClient(_httpClient)
        .SetPath("data/search")
        .SetContent(queryParameter)
        .PostAsync(context.With(_logger))
        .GetContent<BatchQuerySet<DatalakePathItem>>();
}
