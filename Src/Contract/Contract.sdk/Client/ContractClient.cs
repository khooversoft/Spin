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


namespace Contract.sdk.Client;

public class ContractClient : IContractClient
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

    public async Task Set(DocumentId documentId, BlockChainModel blockChainModel, CancellationToken token = default)
    {
        documentId.NotNull();
        blockChainModel.Verify();
        var ls = _logger.LogEntryExit();

        _logger.LogTrace("Id={documentId}", documentId);

        HttpResponseMessage? response = await _httpClient.PostAsJsonAsync($"api/contract/set/{documentId.ToUrlEncoding()}", blockChainModel, token);
        response.NotNull(name: $"{nameof(Set)} failed", logger: _logger);
        response.EnsureSuccessStatusCode();
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

    public async Task Create(BlkHeader blkHeader, CancellationToken token = default)
    {
        blkHeader.Verify();
        var ls = _logger.LogEntryExit();

        Document doc = new DocumentBuilder()
            .SetDocumentId((DocumentId)blkHeader.DocumentId)
            .SetData(blkHeader)
            .Build();

        _logger.LogTrace("Creating contract={id}", blkHeader.DocumentId);
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/contract/create", doc, token);
        response.EnsureSuccessStatusCode();
    }

    public async Task Append(DocumentId documentId, BlkCollection blkTransaction, CancellationToken token = default)
    {
        documentId.NotNull();
        blkTransaction.Verify();
        var ls = _logger.LogEntryExit();

        Document doc = new DocumentBuilder()
            .SetDocumentId(documentId)
            .SetData(blkTransaction)
            .Build();

        _logger.LogTrace("Append 'BlkCollection' to contract={id}", documentId);
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/contract/create", doc, token);
        response.EnsureSuccessStatusCode();
    }

    public async Task Append(DocumentId documentId, ContractBlkCode blkCode, CancellationToken token = default)
    {
        documentId.NotNull();
        blkCode.Verify();
        var ls = _logger.LogEntryExit();

        Document doc = new DocumentBuilder()
            .SetDocumentId(documentId)
            .SetData(blkCode)
            .Build();

        _logger.LogTrace("Append 'BlkCode' to contract={id}", documentId);
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/contract/create", doc, token);
        response.EnsureSuccessStatusCode();
    }


    //  ///////////////////////////////////////////////////////////////////////////////////////
    //  Sign

    public async Task Sign(DocumentId documentId, BlockChainModel blockChainModel, CancellationToken token = default)
    {
        documentId.NotNull();
        blockChainModel.Verify();
        var ls = _logger.LogEntryExit();

        _logger.LogTrace("Sign model for contract={id}", documentId);
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/contract/sign/{documentId.ToUrlEncoding()}", blockChainModel, token);
        response.EnsureSuccessStatusCode();
    }

    public async Task<BlockChainModel> Sign(BlockChainModel blockChainModel, CancellationToken token = default)
    {
        blockChainModel.Verify();
        var ls = _logger.LogEntryExit();

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/contract/sign/model", blockChainModel, token);
        response.EnsureSuccessStatusCode();

        _logger.LogTrace("Sign model");
        string json = await response.Content.ReadAsStringAsync();

        return Json.Default.Deserialize<BlockChainModel>(json)
            .NotNull(name: "Cannot deserialize");
    }

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

    public async Task<bool> Validate(BlockChainModel blockChainModel, CancellationToken token = default)
    {
        blockChainModel.Verify();
        var ls = _logger.LogEntryExit();

        _logger.LogTrace("Validate model");
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/contract/validate", blockChainModel, token);
        if (response.StatusCode == HttpStatusCode.Conflict) return false;

        response.EnsureSuccessStatusCode();
        return true;
    }
}
