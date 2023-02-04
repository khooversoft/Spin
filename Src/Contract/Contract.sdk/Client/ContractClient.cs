using Contract.sdk.Models;
using Contract.sdk.Service;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using Toolbox.Abstractions;
using Toolbox.Abstractions.Protocol;
using Toolbox.Abstractions.Tools;
using Toolbox.Block.Serialization;
using Toolbox.DocumentStore;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Model;
using Toolbox.Monads;
using Toolbox.Tools;

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

    public async Task<Option<T?>> GetLatest<T>(DocumentId documentId, CancellationToken token = default) where T : class
    {
        var list = await Get(documentId, typeof(T).ToBlockTypeRequest(), token);
        if (list == null || list.Count == 0) return Option<T?>.None;

        return list.GetLast<T>().Option();
    }

    public async Task<IReadOnlyList<DataBlockResult>> Get(DocumentId documentId, string blockTypes, CancellationToken token = default)
    {
        documentId.NotNull();
        blockTypes.NotEmpty();
        var ls = _logger.LogEntryExit();

        HttpResponseMessage response = await _httpClient.GetAsync($"api/contract/{documentId.ToUrlEncoding()}/{blockTypes}", token);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadAsStringAsync(token))
            .ToObject<IReadOnlyList<Document>>().NotNull()
            .Select(x => new DataBlockResult(x.ObjectClass, x))
            .ToList();
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

    public async Task<AppendResult> Append(Batch<Document> batch, CancellationToken token)
    {
        batch.NotNull();
        var ls = _logger.LogEntryExit();
        _logger.LogTrace("Append batch={id}", batch.Id);

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/contract/append", batch, token);
        response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync(token);
        var result = content.ToObject<AppendResult>().NotNull(name: "Deserialize error");

        if (result.HasError)
            _logger.LogError("Append batch failed some or all transactions, successCount={successCount}, failCount={failCount}, data={data}", result.SuccessCount, result.ErrorCount, result);
        else
            _logger.LogTrace("Append batch completed, successCount={successCount}, failCount={failCount}, data={data}", result.SuccessCount, result.ErrorCount, result);

        return result;
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
