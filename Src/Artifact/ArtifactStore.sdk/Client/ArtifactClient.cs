using ArtifactStore.sdk.Model;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Model;
using Toolbox.Tools;

namespace ArtifactStore.sdk.Client
{
    public class ArtifactClient : IArtifactClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ArtifactClient> _logger;

        public ArtifactClient(HttpClient httpClient, ILogger<ArtifactClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ArtifactPayload?> Get(ArtifactId id, CancellationToken token = default)
        {
            id.VerifyNotNull(nameof(id));
            _logger.LogTrace($"{nameof(Get)}: Id={id}");

            try
            {
                return await _httpClient.GetFromJsonAsync<ArtifactPayload?>($"api/artifact/{id.ToBase64()}", token);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"{nameof(Get)}: id={id} failed");
                return null;
            }
        }

        public async Task Set(ArtifactPayload articlePayload, CancellationToken token = default)
        {
            articlePayload.VerifyNotNull(nameof(articlePayload));

            _logger.LogTrace($"{nameof(Set)}: Id={articlePayload.Id}");

            HttpResponseMessage message = await _httpClient.PostAsJsonAsync("api/artifact", articlePayload, token);
            message.EnsureSuccessStatusCode();
        }

        public async Task<bool> Delete(ArtifactId id, CancellationToken token = default)
        {
            id.VerifyNotNull(nameof(id));
            _logger.LogTrace($"{nameof(Delete)}: Id={id}");

            HttpResponseMessage response = await _httpClient.DeleteAsync($"api/artifact/{id.ToBase64()}", token);

            return response.StatusCode switch
            {
                HttpStatusCode.OK => true,
                HttpStatusCode.NotFound => false,

                _ => throw new HttpRequestException($"Invalid http code={response.StatusCode}"),
            };
        }

        public BatchSetCursor<string> List(QueryParameter queryParameter) => new BatchSetCursor<string>(_httpClient, "api/artifact/list", queryParameter, _logger);
    }
}