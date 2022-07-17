using Contract.sdk.Models;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using Toolbox.Abstractions;
using Toolbox.Block;
using Toolbox.DocumentStore;
using Toolbox.Logging;
using Toolbox.Model;
using Toolbox.Tools;
using Toolbox.Extensions;

namespace Contract.sdk.Client;

public class ContractClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ContractClient> _logger;

    public ContractClient(HttpClient httpClient, ILogger<ContractClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }


    //  ///////////////////////////////////////////////////////////////////////////////////////
    //  CRUD

    public async Task<BlockChainModel> Get(DocumentId documentId, CancellationToken token = default)
    {
        var ls = _logger.LogEntryExit();

        documentId.NotNull();
        _logger.LogTrace("Id={documentId}", documentId);

        BlockChainModel? model = await _httpClient.GetFromJsonAsync<BlockChainModel>($"api/contract/{documentId.ToUrlEncoding()}", token);
        model.NotNull(name: $"{nameof(Get)} failed", logger: _logger);

        return model;
    }

    public async Task<T?> GetLatest<T>(DocumentId documentId, CancellationToken token = default) where T : class
    {
        Document? document = await GetLatest(documentId, typeof(T).Name, token);
        return document switch
        {
            null => null,
            _ => document.ToObject<T>(),
        };
    }

    public async Task<Document?> GetLatest(DocumentId documentId, string blockType, CancellationToken token = default)
    {
        documentId.NotNull();
        var ls = _logger.LogEntryExit();

        HttpResponseMessage response = await _httpClient.GetAsync($"api/contract/latest/{documentId.ToUrlEncoding()}/{blockType}", token);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadAsStringAsync())?.ToObject<Document>();
    }

    public async Task<bool> Delete(DocumentId documentId, CancellationToken token = default)
    {
        documentId.NotNull();
        var ls = _logger.LogEntryExit();
        _logger.LogTrace("Id={documentId}", documentId);

        HttpResponseMessage response = await _httpClient.DeleteAsync($"api/contract/{documentId.ToUrlEncoding()}", token);

        return response.StatusCode switch
        {
            HttpStatusCode.OK => true,
            HttpStatusCode.NotFound => false,

            _ => throw new HttpRequestException($"Invalid http code={response.StatusCode}"),
        };
    }

    public BatchSetCursor<string> Search(QueryParameter queryParameter) => new BatchSetCursor<string>(_httpClient, "api/contract/search", queryParameter, _logger);


    //  ///////////////////////////////////////////////////////////////////////////////////////
    //  Block chain

    public async Task Create(ContractCreateModel contractCreate, CancellationToken token = default)
    {
        contractCreate.Verify();
        var ls = _logger.LogEntryExit();

        _logger.LogTrace("Creating contract={id}", contractCreate.DocumentId);
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/contract/create", contractCreate, token);
        response.EnsureSuccessStatusCode();
    }

    public async Task Append<T>(DocumentId documentId, T value, string principleId, CancellationToken token = default) where T : class
    {
        documentId.NotNull();
        value.NotNull();
        principleId.NotNull();
        var ls = _logger.LogEntryExit();

        Document doc = new DocumentBuilder()
            .SetDocumentId(documentId)
            .SetData(value)
            .SetObjectClass(value.GetType().Name)
            .SetPrincipleId(principleId)
            .Build();

        _logger.LogTrace("Append to contract={id}", documentId);
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/contract/append", doc, token);
        response.EnsureSuccessStatusCode();
    }


    //  ///////////////////////////////////////////////////////////////////////////////////////
    //  Validate

    public async Task<bool> Validate(DocumentId documentId, CancellationToken token = default)
    {
        documentId.NotNull();
        var ls = _logger.LogEntryExit();

        _logger.LogTrace("Validate contract={id}", documentId);

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/contract/validate/{documentId.ToUrlEncoding()}", "<empty>", token);
        if (response.StatusCode == HttpStatusCode.Conflict) return false;

        response.EnsureSuccessStatusCode();
        return true;
    }
}
