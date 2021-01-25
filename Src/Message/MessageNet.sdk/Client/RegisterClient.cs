using MessageNet.sdk.Models;
using MessageNet.sdk.Protocol;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MessageNet.sdk.Client
{
    public class RegisterClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RegisterClient> _logger;

        public RegisterClient(HttpClient httpClient, ILogger<RegisterClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> Register(EndpointId endpointId, Uri callbackUri)
        {
            _logger.LogTrace($"{nameof(Register)}: EndpointId={endpointId}, Uri={callbackUri}");

            var registerSync = new RegisterSync
            {
                EndpointId = endpointId,
                CallbackUri = callbackUri.ToString(),
            };

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/register", registerSync);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> Remove(EndpointId endpointId)
        {
            _logger.LogTrace($"{nameof(Remove)}: EndpointId={endpointId}");

            HttpResponseMessage response = await _httpClient.DeleteAsync($"api/register/{endpointId.ToBase64()}");
            return response.IsSuccessStatusCode;
        }
    }
}