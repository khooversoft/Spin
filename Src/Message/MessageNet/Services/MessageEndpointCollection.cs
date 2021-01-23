using MessageNet.Application;
using MessageNet.sdk.Host;
using MessageNet.sdk.Protocol;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Toolbox.Tools;

namespace MessageNet.Services
{
    public class MessageEndpointCollection
    {
        private readonly ILogger<MessageEndpointCollection> _logger;
        private readonly HttpClient _httpClient;
        private readonly IMessageHost _messageHost;
        private readonly ConcurrentDictionary<string, Registration> _registrations = new ConcurrentDictionary<string, Registration>(StringComparer.OrdinalIgnoreCase);

        public MessageEndpointCollection(HttpClient httpClient, IMessageHost messageHost, ILogger<MessageEndpointCollection> logger)
        {
            _httpClient = httpClient;
            _messageHost = messageHost;
            _logger = logger;
        }

        public bool Register(EndpointId endpointId, Uri callbackUrl)
        {
            endpointId.VerifyNotNull(nameof(endpointId));
            callbackUrl.VerifyNotNull(nameof(callbackUrl));

            bool added = false;

            _ = _registrations.GetOrAdd((string)endpointId, _ =>
            {
                added = true;
                _logger.LogTrace($"{nameof(Register)}: EndpointId={endpointId}");

                var registration = new Registration(_httpClient, endpointId, callbackUrl, _logger);

                _messageHost.StartReceiver((string)endpointId, registration.Outboud.SendAsync);

                return registration;
            });

            return added;
        }

        public async Task<bool> Unregister(EndpointId endpointId)
        {
            endpointId.VerifyNotNull(nameof(endpointId));

            _logger.LogTrace($"{nameof(Unregister)}: EndpointId={endpointId}");

            if (!_registrations.TryRemove((string)endpointId, out Registration? registration)) return false;

            await _messageHost.StopReceiver((string)endpointId);

            registration.Outboud.Complete();

            return true;
        }

        private record Registration
        {
            private readonly HttpClient _httpClient;
            private readonly ILogger _logger;

            public Registration(HttpClient httpClient, EndpointId endpointId, Uri callbackUrl, ILogger logger)
            {
                _httpClient = httpClient;

                EndpointId = endpointId;
                CallbackUrl = callbackUrl;
                _logger = logger;
                Outboud = new ActionBlock<MessagePacket>(SendToUrl);
            }

            public Uri CallbackUrl { get; } = null!;
            public EndpointId EndpointId { get; } = null!;
            public ActionBlock<MessagePacket> Outboud { get; } = null!;

            public async Task SendToUrl(MessagePacket messagePacket)
            {
                string msg = $"{nameof(SendToUrl)}: Sending message packet to {CallbackUrl}, message id={messagePacket.GetMessage()!.MessageId}";
                _logger.LogTrace(msg);

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(CallbackUrl, messagePacket);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogTrace($"{msg} - completed");
                    return;
                }

                string json = await response.Content.ReadAsStringAsync();
                _logger.LogError($"{msg} - Failed, response={json}");
            }
        }
    }
}