using Bank.sdk.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Document;
using Toolbox.Model;
using Toolbox.Tools;

namespace Bank.sdk.Client;

public class BankAccountClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BankAccountClient> _logger;

    public BankAccountClient(HttpClient httpClient, ILogger<BankAccountClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> Delete(DocumentId documentId, CancellationToken token = default)
    {
        _logger.LogTrace($"Delete directoryId={documentId}");

        HttpResponseMessage response = await _httpClient.DeleteAsync($"api/account/{documentId.ToUrlEncoding()}", token);
        if (response.StatusCode == HttpStatusCode.NotFound) return false;

        response.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<BankAccount?> Get(DocumentId documentId, CancellationToken token = default)
    {
        _logger.LogTrace($"Getting directoryId={documentId}");

        try
        {
            return await _httpClient.GetFromJsonAsync<BankAccount>($"api/account/{documentId.ToUrlEncoding()}", token);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task Set(BankAccount entry, CancellationToken token = default)
    {
        _logger.LogTrace($"Putting entry directoryId={entry.AccountId}");

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/account", entry, token);
        response.EnsureSuccessStatusCode();
    }

    public BatchSetCursor<DatalakePathItem> Search(QueryParameter query) => new BatchSetCursor<DatalakePathItem>(_httpClient, "api/account/search", query, _logger);
}
