using ArtifactStore.sdk.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Model;
using Toolbox.Tools;

namespace Identity.sdk.Client
{
    public abstract class ClientBase<T> where T : class
    {
        private readonly HttpClient _httpClient;
        private readonly string _path;
        private readonly ILogger _logger;

        public ClientBase(HttpClient httpClient, string path, ILogger logger)
        {
            httpClient.VerifyNotNull(nameof(httpClient));
            path.VerifyNotEmpty(nameof(path));
            logger.VerifyNotNull(nameof(logger));

            _httpClient = httpClient;
            _path = path;
            _logger = logger;
        }

        public async Task Set(T subject, CancellationToken token)
        {
            subject.VerifyNotNull(nameof(subject));

            _logger.LogTrace($"{nameof(Set)}: subject={subject}");

            HttpResponseMessage message = await _httpClient.PostAsJsonAsync($"api/{_path}", subject, token);
            message.EnsureSuccessStatusCode();
        }

        public BatchSetCursor<string> List(QueryParameter queryParameter) => new BatchSetCursor<string>(_httpClient, $"api/{_path}/list", queryParameter, _logger);

        protected async Task<T?> Get(ArtifactId artifactId, CancellationToken token)
        {
            artifactId.VerifyNotNull(nameof(artifactId));
            _logger.LogTrace($"{nameof(Get)}: artifactId={artifactId}");

            try
            {
                return await _httpClient.GetFromJsonAsync<T?>($"api/{artifactId.ToBase64()}", token);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Get: id={artifactId} failed");
                return null;
            }
        }

        protected async Task<bool> Delete(ArtifactId artifactId, CancellationToken token)
        {
            artifactId.VerifyNotNull(nameof(artifactId));

            _logger.LogTrace($"{nameof(Delete)}: artifactId={artifactId}");

            HttpResponseMessage response = await _httpClient.DeleteAsync($"api/{artifactId.ToBase64()}", token);
            _logger.LogTrace($"{nameof(Delete)}: response: {response.StatusCode} for artifactId={artifactId}");

            return response.StatusCode switch
            {
                HttpStatusCode.OK => true,
                HttpStatusCode.NotFound => false,

                _ => throw new HttpRequestException($"Invalid http code={response.StatusCode}"),
            };
        }
    }
}
