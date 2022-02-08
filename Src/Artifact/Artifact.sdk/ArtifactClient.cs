﻿using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Document;
using Toolbox.Model;
using Toolbox.Tools;

namespace Artifact.sdk
{
    public class ArtifactClient
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
            id.VerifyNotNull(nameof(id));
            _logger.LogTrace($"{nameof(Get)}: Id={id}");

            try
            {
                return await _httpClient.GetFromJsonAsync<Document?>($"api/artifact/{id.ToUrlEncoding()}", token);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"{nameof(Get)}: id={id} failed");
                return null;
            }
        }

        public async Task Set(Document document, CancellationToken token = default)
        {
            document.VerifyNotNull(nameof(document));

            _logger.LogTrace($"{nameof(Set)}: Id={document.DocumentId}");

            HttpResponseMessage message = await _httpClient.PostAsJsonAsync("api/artifact", document, token);
            message.EnsureSuccessStatusCode();
        }

        public async Task<bool> Delete(DocumentId id, CancellationToken token = default)
        {
            id.VerifyNotNull(nameof(id));
            _logger.LogTrace($"{nameof(Delete)}: Id={id}");

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
}