using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Abstractions;
using Toolbox.Abstractions.Models;
using Toolbox.Abstractions.Protocol;
using Toolbox.Abstractions.Tools;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Extensions;
using Toolbox.Logging;
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
        id.NotNull();
        var ls = _logger.LogEntryExit();

        _logger.LogTrace("Get Id={id}", id);

        HttpResponseMessage message = await _httpClient.GetAsync($"api/artifact/{id.ToUrlEncoding()}", token);
        _logger.LogTrace("Getting {id}, statusCode={statusCode}", id, message.StatusCode);

        if (message.StatusCode == HttpStatusCode.NotFound) return null;
        message.EnsureSuccessStatusCode();

        string content = await message.Content.ReadAsStringAsync();
        return content.ToObject<Document>();
    }

    public async Task Set(Document document, CancellationToken token = default)
    {
        document.NotNull();
        var ls = _logger.LogEntryExit();

        _logger.LogTrace("Set Id={documentId}", args: document.DocumentId);

        HttpResponseMessage message = await _httpClient.PostAsJsonAsync("api/artifact", document, token);
        message.EnsureSuccessStatusCode();
    }

    public async Task<bool> Delete(DocumentId id, CancellationToken token = default)
    {
        id.NotNull();
        var ls = _logger.LogEntryExit();
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
