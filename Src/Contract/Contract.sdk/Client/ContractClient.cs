using Contract.sdk.Models;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Block;
using Toolbox.Document;
using Toolbox.Extensions;
using Toolbox.Model;
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
        BlockChainModel? model = await _httpClient.GetFromJsonAsync<BlockChainModel>($"api/contract/{documentId.ToUrlEncoding()}", token);
        model.VerifyNotNull("Null returned");

        return model;
    }

    public async Task Set(DocumentId documentId, BlockChainModel blockChainModel, CancellationToken token = default)
    {
        HttpResponseMessage? response = await _httpClient.PostAsJsonAsync($"api/contract/set/{documentId.ToUrlEncoding()}", blockChainModel, token);
        response.VerifyNotNull("Null returned");
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> Delete(DocumentId documentId, CancellationToken token = default)
    {
        documentId.VerifyNotNull(nameof(documentId));
        _logger.LogTrace($"{nameof(Delete)}: Id={documentId}");

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

        Document doc = new DocumentBuilder()
            .SetDocumentId((DocumentId)blkHeader.DocumentId)
            .SetData(blkHeader)
            .Build();

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/contract/create", doc, token);
        response.EnsureSuccessStatusCode();
    }

    public async Task Append(DocumentId documentId, BlkTransaction blkTransaction, CancellationToken token = default)
    {
        blkTransaction.Verify();

        Document doc = new DocumentBuilder()
            .SetDocumentId(documentId)
            .SetData(blkTransaction)
            .Build();

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/contract/create", doc, token);
        response.EnsureSuccessStatusCode();
    }

    public async Task Append(DocumentId documentId, BlkCode blkCode, CancellationToken token = default)
    {
        blkCode.Verify();

        Document doc = new DocumentBuilder()
            .SetDocumentId(documentId)
            .SetData(blkCode)
            .Build();

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/contract/create", doc, token);
        response.EnsureSuccessStatusCode();
    }


    //  ///////////////////////////////////////////////////////////////////////////////////////
    //  Sign

    public async Task Sign(DocumentId documentId, BlockChainModel blockChainModel, CancellationToken token = default)
    {
        blockChainModel.Verify();

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/contract/sign/{documentId.ToUrlEncoding()}", blockChainModel, token);
        response.EnsureSuccessStatusCode();
    }

    public async Task<BlockChainModel> Sign(BlockChainModel blockChainModel, CancellationToken token = default)
    {
        blockChainModel.Verify();

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/contract/sign/model", blockChainModel, token);
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();

        return Json.Default.Deserialize<BlockChainModel>(json)
            .VerifyNotNull("Cannot deserialize");
    }

    public async Task<bool> Validate(DocumentId documentId, CancellationToken token = default)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/contract/validate/{documentId.ToUrlEncoding()}", "<empty>", token);
        if (response.StatusCode == HttpStatusCode.Conflict) return false;

        response.EnsureSuccessStatusCode();
        return true;
    }

    public async Task<bool> Validate(BlockChainModel blockChainModel, CancellationToken token = default)
    {
        blockChainModel.Verify();

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/contract/validate", blockChainModel, token);
        if (response.StatusCode == HttpStatusCode.Conflict) return false;

        response.EnsureSuccessStatusCode();
        return true;
    }
}
