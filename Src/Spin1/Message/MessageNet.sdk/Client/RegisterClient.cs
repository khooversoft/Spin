//using MessageNet.sdk.Models;
//using MessageNet.sdk.Protocol;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Net.Http;
//using System.Net.Http.Json;
//using System.Threading.Tasks;

//namespace MessageNet.sdk.Client
//{
//    public class RegisterClient
//    {
//        private readonly HttpClient _httpClient;
//        private readonly ILogger<RegisterClient> _logger;

//        public RegisterClient(HttpClient httpClient, ILogger<RegisterClient> logger)
//        {
//            _httpClient = httpClient;
//            _logger = logger;
//        }

//        public async Task<bool> Register(MessageUrl url, Uri callbackUri)
//        {
//            _logger.LogTrace($"{nameof(Register)}: Url={url}, Callback Uri={callbackUri}");

//            var registerSync = new RegisterSync
//            {
//                Url = url,
//                CallbackUri = callbackUri.ToString(),
//            };

//            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/register", registerSync);
//            return response.IsSuccessStatusCode;
//        }

//        public async Task<bool> Remove(MessageUrl url)
//        {
//            _logger.LogTrace($"{nameof(Remove)}: EndpointId={url}");

//            HttpResponseMessage response = await _httpClient.DeleteAsync($"api/register/{url.ToBase64()}");
//            return response.IsSuccessStatusCode;
//        }
//    }
//}