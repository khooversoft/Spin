using Directory.sdk.Service;
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
using Toolbox.Azure.DataLake.Model;
using Toolbox.Document;
using Toolbox.Extensions;
using Toolbox.Model;
using Toolbox.Tools;

namespace Directory.sdk.Client
{
    public class DirectoryClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DirectoryClient> _logger;

        public DirectoryClient(HttpClient httpClient, ILogger<DirectoryClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<DirectoryEntry?> Get(DocumentId documentId, CancellationToken token = default)
        {
            _logger.LogTrace($"Getting directoryId={documentId}");

            try
            {
                return await _httpClient.GetFromJsonAsync<DirectoryEntry>($"api/entry/{documentId.ToUrlEncoding()}", token);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task Set(DirectoryEntry entry, CancellationToken token = default)
        {
            _logger.LogTrace($"Putting entry directoryId={entry.DirectoryId}");

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/entry", entry, token);
            response.EnsureSuccessStatusCode();
        }

        public async Task Delete(DocumentId documentId, CancellationToken token = default)
        {
            _logger.LogTrace($"Delete directoryId={documentId}");

            HttpResponseMessage response = await _httpClient.DeleteAsync($"api/entry/{documentId.ToUrlEncoding()}", token);
            response.EnsureSuccessStatusCode();
        }

        public BatchSetCursor<DatalakePathItem> Search(QueryParameter query) => new BatchSetCursor<DatalakePathItem>(_httpClient, "api/entry/search", query, _logger);
    }
}
