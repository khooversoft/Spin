using Directory.sdk.Model;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Block;
using Toolbox.Logging;
using Toolbox.Tools;

namespace Directory.sdk.Client
{
    public class SigningClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SigningClient> _logger;

        public SigningClient(HttpClient httpClient, ILogger<SigningClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<SignRequestResponse> Sign(SignRequest signRequest, CancellationToken token = default)
        {
            var ls = _logger.LogEntryExit();
            _logger.LogTrace("Signing request for id={id}", signRequest.Id);

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/signing/sign", signRequest, token);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();

            return Json.Default.Deserialize<SignRequestResponse>(json).NotNull();
        }

        public async Task<bool> Validate(ValidateRequest validateRequest, CancellationToken token = default)
        {
            var ls = _logger.LogEntryExit();
            _logger.LogTrace("Signing request for id={id}", validateRequest.Id);

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync($"api/signing/validate", validateRequest, token);
            return response.StatusCode == HttpStatusCode.OK ? true : false;
        }
    }
}
