//using MessageNet.sdk.Protocol;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Net.Http.Json;
//using System.Text;
//using System.Threading.Tasks;
//using System.Threading.Tasks.Dataflow;
//using Toolbox.Tools;

//namespace MessageNet.sdk.Endpoint
//{
//    public class HttpCallback : ICallback
//    {
//        private readonly HttpClient _httpClient;
//        private readonly EndpointId _endpointId;
//        private readonly Uri _callbackUrl;
//        private readonly ILogger<HttpCallback> _logger;

//        public HttpCallback(HttpClient httpClient, EndpointId endpointId, Uri callbackUrl, ILogger<HttpCallback> logger)
//        {
//            httpClient.VerifyNotNull(nameof(httpClient));
//            endpointId.VerifyNotNull(nameof(endpointId));
//            callbackUrl.VerifyNotNull(nameof(callbackUrl));
//            logger.VerifyNotNull(nameof(logger));

//            _httpClient = httpClient;
//            _endpointId = endpointId;
//            _callbackUrl = callbackUrl;
//            _logger = logger;
//        }

//        public async Task<(bool ok, string? message)> Send(MessagePacket messagePacket)
//        {
//            string msg = $"{nameof(Send)}: Sending message packet to {_callbackUrl}, message id={messagePacket.GetMessage()?.MessageId ?? Guid.Empty}";
//            _logger.LogTrace(msg);

//            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_callbackUrl, messagePacket);

//            if (response.IsSuccessStatusCode)
//            {
//                _logger.LogTrace($"{msg} - completed");
//                return (true, null);
//            }

//            string json = await response.Content.ReadAsStringAsync();
//            msg += $" - Failed, response={json}";
//            _logger.LogError(msg);

//            return (false, msg);
//        }
//    }
//}
