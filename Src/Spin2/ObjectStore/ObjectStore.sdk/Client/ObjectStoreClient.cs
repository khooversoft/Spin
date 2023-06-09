using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.DataLake;
using Toolbox.DocumentContainer;
using Toolbox.Extensions;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Types.Maybe;

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

    public async Task<Option<Document>> Read(string objectId, ScopeContext context) => await new RestClient(_httpClient)
        .SetPath($"data/{objectId}")
        .SetLogger(_logger)
        .GetAsync(context)
        .GetContent<Document>(context);

    public async Task<Option<ETag>> Write(Document document, ScopeContext context) => await new RestClient(_httpClient)
        .SetPath("data")
        .SetLogger(_logger)
        .SetContent(document)
        .PostAsync(context)
        .GetContent<ETag>(context);

    public async Task<StatusCode> Delete(string objectId, ScopeContext context) => await new RestClient(_httpClient)
        .SetPath($"data/{objectId}")
        .SetPath(objectId)
        .SetLogger(_logger)
        .DeleteAsync(context)
        .GetStatusCode();

    public async Task<Option<BatchQuerySet<DatalakePathItem>>> Search(QueryParameter queryParameter, ScopeContext context) => await new RestClient(_httpClient)
        .SetPath("data/search")
        .SetLogger(_logger)
        .SetContent(queryParameter)
        .PostAsync(context)
        .GetContent<BatchQuerySet<DatalakePathItem>>(context);
}
