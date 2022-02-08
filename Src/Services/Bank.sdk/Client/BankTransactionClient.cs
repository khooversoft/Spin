using Bank.sdk.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Document;
using Toolbox.Tools;

namespace Bank.sdk.Client;

public class BankTransactionClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BankTransactionClient> _logger;

    public BankTransactionClient(HttpClient httpClient, ILogger<BankTransactionClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<TrxBalance?> GetBalance(DocumentId documentId, CancellationToken token = default)
    {
        _logger.LogTrace($"Getting directoryId={documentId}");

        try
        {
            return await _httpClient.GetFromJsonAsync<TrxBalance>($"api/transaction/balance/{documentId.ToUrlEncoding()}", token);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<TrxBatch<TrxRequestResponse>> Set(TrxBatch<TrxRequest> batch, CancellationToken token = default)
    {
        _logger.LogTrace($"Posting accountId={batch.Id}");

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/transaction", batch, token);
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();

        return Json.Default.Deserialize<TrxBatch<TrxRequestResponse>>(json)
            .VerifyNotNull("Deserialize failed");
    }
}
