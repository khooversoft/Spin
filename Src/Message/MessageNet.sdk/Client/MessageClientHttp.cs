//using MessageNet.sdk.Models;
//using MessageNet.sdk.Protocol;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Net;
//using System.Net.Http;
//using System.Net.Http.Json;
//using System.Threading;
//using System.Threading.Tasks;
//using Toolbox.Extensions;
//using Toolbox.Model;
//using Toolbox.Tools;

//namespace MessageNet.sdk.Client
//{
//    public class MessageClient
//    {
//        private readonly HttpClient _httpClient;
//        private readonly ILogger<MessageClient> _logger;

//        public MessageClient(HttpClient httpClient, ILogger<MessageClient> logger)
//        {
//            _httpClient = httpClient;
//            _logger = logger;
//        }

//        public async Task<bool> Send(MessagePacket messagePacket)
//        {
//            _logger.LogTrace($"{nameof(Send)}: Sending packet, id={messagePacket.GetMessage()?.MessageId ?? Guid.Empty}");

//            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/message/send", messagePacket);
//            string context = await response.Content.ReadAsStringAsync();
//            return response.IsSuccessStatusCode;
//        }

//        public async Task<MessagePacket?> Call(MessagePacket messagePacket)
//        {
//            _logger.LogTrace($"{nameof(Call)}: Calling packet, id={messagePacket.GetMessage()?.MessageId ?? Guid.Empty}");

//            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/message/call", messagePacket);
//            if (!response.IsSuccessStatusCode) return null;

//            string json = await response.Content.ReadAsStringAsync();
//            _logger.LogTrace($"{nameof(Call)}: received calling packet, id={messagePacket.GetMessage()?.MessageId ?? Guid.Empty}");

//            return Json.Default.Deserialize<MessagePacket>(json);
//        }
//    }
//}