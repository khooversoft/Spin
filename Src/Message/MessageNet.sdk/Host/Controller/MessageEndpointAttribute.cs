using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace MessageNet.sdk.Host
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public class MessageEndpointAttribute : Attribute
    {
        public MessageEndpointAttribute(string endpoint)
        {
            endpoint.VerifyNotEmpty(nameof(endpoint));

            Endpoint = endpoint;
        }

        public string Endpoint { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class MessageGet : MessageEndpointAttribute
    {
        public MessageGet() : base("get") { }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class MessagePost : MessageEndpointAttribute
    {
        public MessagePost() : base("post") { }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class MessageDelete : MessageEndpointAttribute
    {
        public MessageDelete() : base("delete") { }
    }
}
