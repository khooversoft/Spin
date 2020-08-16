using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Toolbox.Tools
{
    public class RestResponse
    {
        public RestResponse(HttpResponseMessage httpResponseMessage)
        {
            HttpResponseMessage = httpResponseMessage;
        }

        public HttpResponseMessage HttpResponseMessage { get; }
    }

    public class RestResponse<T> : RestResponse
    {
        public RestResponse(HttpResponseMessage httpResponseMessage, T content, string contentAsString)
            : base(httpResponseMessage)
        {
            Content = content;
            ContentAsString = contentAsString;
        }

        public T Content { get; }
        public string ContentAsString { get; }
    }
}
