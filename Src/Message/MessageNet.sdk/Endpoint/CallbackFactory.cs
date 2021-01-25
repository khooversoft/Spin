using MessageNet.sdk.Protocol;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace MessageNet.sdk.Endpoint
{
    public class CallbackFactory : ICallbackFactory
    {
        private readonly HttpClient _httpClient;
        private readonly ILoggerFactory _loggerFactory;

        public CallbackFactory(HttpClient httpClient, ILoggerFactory loggerFactory)
        {
            _httpClient = httpClient;
            _loggerFactory = loggerFactory;
        }

        public ICallback Create(EndpointId endpointId, Uri callbackUrl) => 
            new HttpCallback(_httpClient, endpointId, callbackUrl, _loggerFactory.CreateLogger<HttpCallback>());
    }
}
