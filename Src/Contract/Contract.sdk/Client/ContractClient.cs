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

namespace Artifact.sdk
{
    public class ContractClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ContractClient> _logger;

        public ContractClient(HttpClient httpClient, ILogger<ContractClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> Delete(DocumentId id, CancellationToken token = default)
        {
            id.VerifyNotNull(nameof(id));
            _logger.LogTrace($"{nameof(Delete)}: Id={id}");

            HttpResponseMessage response = await _httpClient.DeleteAsync($"api/contract/{id.ToUrlEncoding()}", token);

            return response.StatusCode switch
            {
                HttpStatusCode.OK => true,
                HttpStatusCode.NotFound => false,

                _ => throw new HttpRequestException($"Invalid http code={response.StatusCode}"),
            };
        }

        //public async Task<BlockChainDocument?> Get(DocumentId id, CancellationToken token = default)
        //{
        //    id.VerifyNotNull(nameof(id));
        //    _logger.LogTrace($"{nameof(Get)}: Id={id}");

        //    try
        //    {
        //        return await _httpClient.GetFromJsonAsync<BlockChainDocument?>($"api/contract/{id.ToUrlEncoding()}", token);
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        _logger.LogError(ex, $"{nameof(Get)}: id={id} failed");
        //        return null;
        //    }
        //}

        public async Task Set(Document document, CancellationToken token = default)
        {
            document.VerifyNotNull(nameof(document));

            _logger.LogTrace($"{nameof(Set)}: Id={document.DocumentId}");

            HttpResponseMessage message = await _httpClient.PostAsJsonAsync("api/contract", document, token);
            message.EnsureSuccessStatusCode();
        }


        public BatchSetCursor<string> Search(QueryParameter queryParameter) => new BatchSetCursor<string>(_httpClient, "api/artifact/search", queryParameter, _logger);
    }
}