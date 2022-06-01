using Bank.Abstractions;
using Bank.Abstractions.Model;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using Toolbox.Tools;

namespace Bank.sdk.Client;

public class BankClearingClient : IBankClearingClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BankClearingClient> _logger;

    public BankClearingClient(HttpClient httpClient, ILogger<BankClearingClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<TrxBatch<TrxRequestResponse>> Send(TrxBatch<TrxRequest> batch, CancellationToken token = default)
    {
        _logger.LogTrace("Posting accountId={batch.Id}", batch.Id);

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/clearing", batch, token);
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();

        return Json.Default.Deserialize<TrxBatch<TrxRequestResponse>>(json)
            .NotNull(name: "Deserialize failed");
    }
}
