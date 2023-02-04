using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Model;
using Toolbox.Tools;

namespace Spin.Common.Client
{
    public class PingClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PingClient> _logger;

        public PingClient(HttpClient httpClient, ILogger<PingClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<(bool Ok, PingResponse? Response)> Ping()
        {
            _logger.LogTrace($"{nameof(Ping)}: pinging");

            HttpResponseMessage response = await _httpClient.GetAsync("api/ping");

            PingResponse? pingResponse = (await response.Content.ReadAsStringAsync())
                .Func(x => x.IsEmpty() ? null : Json.Default.Deserialize<PingResponse>(x));

            return (response.StatusCode == HttpStatusCode.OK, pingResponse);
        }

        public async Task<(bool Ok, PingResponse? Response)> Ready()
        {
            _logger.LogTrace($"{nameof(Ready)}: pinging ready status");

            HttpResponseMessage response = await _httpClient.GetAsync("api/ping/ready");

            PingResponse? pingResponse = (await response.Content.ReadAsStringAsync())
                .Func(x => x.IsEmpty() ? null : Json.Default.Deserialize<PingResponse>(x));

            return (response.StatusCode == HttpStatusCode.OK, pingResponse);
        }

        public async Task<PingLogs?> GetLogs() => await _httpClient.GetFromJsonAsync<PingLogs>("api/ping/logs");
    }
}