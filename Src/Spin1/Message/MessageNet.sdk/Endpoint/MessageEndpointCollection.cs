//using MessageNet.sdk.Host;
//using MessageNet.sdk.Protocol;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Concurrent;
//using System.Net.Http;
//using System.Net.Http.Json;
//using System.Threading.Tasks;
//using System.Threading.Tasks.Dataflow;
//using Toolbox.Tools;

//namespace MessageNet.sdk.Endpoint
//{
//    public class MessageEndpointCollection
//    {
//        private readonly ILogger<MessageEndpointCollection> _logger;
//        private readonly IMessageHost _messageHost;
//        private readonly ICallbackFactory _callbackFactory;
//        private readonly ConcurrentDictionary<string, ICallback> _registrations = new ConcurrentDictionary<string, ICallback>(StringComparer.OrdinalIgnoreCase);

//        public MessageEndpointCollection(IMessageHost messageHost, ICallbackFactory callbackFactory, ILogger<MessageEndpointCollection> logger)
//        {
//            _messageHost = messageHost;
//            _callbackFactory = callbackFactory;
//            _logger = logger;
//        }

//        public bool Register(EndpointId endpointId, Uri callbackUrl)
//        {
//            endpointId.VerifyNotNull(nameof(endpointId));
//            callbackUrl.VerifyNotNull(nameof(callbackUrl));

//            bool added = false;

//            _ = _registrations.GetOrAdd((string)endpointId, _ =>
//            {
//                added = true;
//                _logger.LogTrace($"{nameof(Register)}: EndpointId={endpointId}");

//                ICallback registration = _callbackFactory.Create(endpointId, callbackUrl);

//                _messageHost.Receiver.Start((ServiceId)endpointId, x => registration.Send(x));

//                return registration;
//            });

//            return added;
//        }

//        public async Task<bool> Remove(EndpointId endpointId)
//        {
//            endpointId.VerifyNotNull(nameof(endpointId));

//            _logger.LogTrace($"{nameof(Remove)}: EndpointId={endpointId}");

//            bool status = _registrations.TryRemove((string)endpointId, out ICallback? _);
//            await _messageHost.Receiver.Stop((ServiceId)endpointId);

//            return status;
//        }
//    }
//}