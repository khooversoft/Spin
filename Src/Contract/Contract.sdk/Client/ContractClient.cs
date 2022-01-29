using Contract.sdk.Models;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Block;
using Toolbox.Document;
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

    public async Task Create(BlkHeader blkHeader, CancellationToken token = default)
    {
        blkHeader.Verify();

        Document doc = new DocumentBuilder()
            .SetDocumentId((DocumentId)blkHeader.DocumentId)
            .SetData(blkHeader)
            .Build();

        bool status = doc.IsHashVerify();

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/contract/create", doc, token);
        response.EnsureSuccessStatusCode();
    }

    public async Task<BlockChainModel> Get(DocumentId documentId, CancellationToken token = default)
    {
        BlockChainModel? model = await _httpClient.GetFromJsonAsync<BlockChainModel>($"api/contract/{documentId.ToUrlEncoding()}", token);
        model.VerifyNotNull("Null returned");

        return model;
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

    public BatchSetCursor<string> Search(QueryParameter queryParameter) => new BatchSetCursor<string>(_httpClient, "api/contract/search", queryParameter, _logger);
}
