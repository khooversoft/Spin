using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageNet.sdk.Host
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class MessageControllerAttribute : Attribute
    {
        public MessageControllerAttribute() { }

        public MessageControllerAttribute(string basePath) => BasePath = basePath;

        public string? BasePath { get; init; }
    }
}
