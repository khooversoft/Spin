using Directory.sdk.Service;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Abstractions.Models;
using Toolbox.Abstractions.Protocol;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Tools;

namespace Directory.sdk.Client
{
    public class IdentityClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IdentityClient> _logger;

        public IdentityClient(HttpClient httpClient, ILogger<IdentityClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> Create(IdentityEntryRequest identityEntryRequest, CancellationToken token = default)
        {
            _logger.LogTrace($"Create directoryId={identityEntryRequest.DirectoryId}");

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/identity/create", value: identityEntryRequest, cancellationToken: token);
            if (response.StatusCode == HttpStatusCode.Conflict) return false;

            response.EnsureSuccessStatusCode();
            return true;
        }

        public async Task<bool> Delete(DocumentId documentId, CancellationToken token = default)
        {
            _logger.LogTrace($"Delete directoryId={documentId}");

            HttpResponseMessage response = await _httpClient.DeleteAsync($"api/identity/{documentId.ToUrlEncoding()}", token);
            if( response.StatusCode == HttpStatusCode.NotFound ) return false;

            response.EnsureSuccessStatusCode();
            return true;
        }

        public async Task<IdentityEntry?> Get(DocumentId documentId, CancellationToken token = default)
        {
            _logger.LogTrace($"Getting directoryId={documentId}");

            try
            {
                return await _httpClient.GetFromJsonAsync<IdentityEntry>($"api/identity/{documentId.ToUrlEncoding()}", token);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task Set(IdentityEntry entry, CancellationToken token = default)
        {
            _logger.LogTrace($"Putting entry directoryId={entry.DirectoryId}");

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/identity", entry, token);
            response.EnsureSuccessStatusCode();
        }

        public BatchSetCursor<DatalakePathItem> Search(QueryParameter query) => new BatchSetCursor<DatalakePathItem>(_httpClient, "api/identity/search", query, _logger);
    }
}
