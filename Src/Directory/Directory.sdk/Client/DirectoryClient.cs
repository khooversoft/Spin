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
using Toolbox.Application;
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

        public async Task<bool> Delete(DocumentId documentId, CancellationToken token = default)
        {
            _logger.LogTrace($"Delete directoryId={documentId}");

            HttpResponseMessage response = await _httpClient.DeleteAsync($"api/entry/{documentId.ToUrlEncoding()}", token);
            if (response.StatusCode == HttpStatusCode.NotFound) return false;

            response.EnsureSuccessStatusCode();
            return true;
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

        public BatchSetCursor<DatalakePathItem> Search(QueryParameter query) => new BatchSetCursor<DatalakePathItem>(_httpClient, "api/entry/search", query, _logger);

        public static async Task<T> Run<T>(string hostUrl, string apiKey, Func<DirectoryClient, Task<T>> action, ILoggerFactory? loggerFactory = null)
        {
            hostUrl.VerifyNotEmpty(nameof(hostUrl));
            apiKey.VerifyNotEmpty(nameof(apiKey));
            action.VerifyNotNull(nameof(action));

            ILoggerFactory factory = loggerFactory ?? LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
            });

            ILogger<DirectoryClient> logger = factory.CreateLogger<DirectoryClient>();

            try
            {
                using var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(hostUrl);
                httpClient.DefaultRequestHeaders.Add(Constants.ApiKeyName, apiKey);

                var client = new DirectoryClient(httpClient, factory.CreateLogger<DirectoryClient>());
                return await action(client);
            }
            catch(HttpRequestException ex)
            {
                logger.LogError(ex, $"Cannot connect to directory {hostUrl}");
                throw;
            }
        }
    }
}
