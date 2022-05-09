using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Abstractions;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Model;
using Toolbox.Tools;

namespace Artifact.sdk;

public class ArtifactClient : IArtifactClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ArtifactClient> _logger;

    public ArtifactClient(HttpClient httpClient, ILogger<ArtifactClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Document?> Get(DocumentId id, CancellationToken token = default)
    {
        id.NotNull(nameof(id));

        _logger.LogTrace("Get Id={id}", id);

        try
        {
            return await _httpClient.GetFromJsonAsync<Document?>($"api/artifact/{id.ToUrlEncoding()}", token);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Get id={id} failed", id);
            return null;
        }
    }

    public async Task Set(Document document, CancellationToken token = default)
    {
        document.NotNull(nameof(document));

        _logger.LogTrace("Set Id={documentId}", args: document.DocumentId);

        HttpResponseMessage message = await _httpClient.PostAsJsonAsync("api/artifact", document, token);
        message.EnsureSuccessStatusCode();
    }

    public async Task<bool> Delete(DocumentId id, CancellationToken token = default)
    {
        id.NotNull(nameof(id));
        _logger.LogTrace("Delete Id={id}", args: id);

        HttpResponseMessage response = await _httpClient.DeleteAsync($"api/artifact/{id.ToUrlEncoding()}", token);

        return response.StatusCode switch
        {
            HttpStatusCode.OK => true,
            HttpStatusCode.NotFound => false,

            _ => throw new HttpRequestException($"Invalid http code={response.StatusCode}"),
        };
    }

    public BatchSetCursor<DatalakePathItem> Search(QueryParameter queryParameter) =>
        new BatchSetCursor<DatalakePathItem>(_httpClient, "api/artifact/search", queryParameter, _logger);
}
