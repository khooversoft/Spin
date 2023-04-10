using System.Net;
using SpinNet.sdk.Model;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace SpinNet.sdk.Application;

public class NetResponseBuilder
{
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
    public string? Message { get; set; }
    public IList<Payload> Payloads { get; set; } = new List<Payload>();
    public IList<KeyValuePair<string, string>> Headers { get; } = new List<KeyValuePair<string, string>>();

    public NetResponseBuilder SetStatusCode(HttpStatusCode statusCode) => this.Action(x => x.StatusCode = statusCode);
    public NetResponseBuilder SetMessage(string message) => this.Action(x => x.Message = message);
    public NetResponseBuilder AddContent(Payload payload) => this.Action(x => x.Payloads.Add(payload));
    public NetResponseBuilder AddContent<T>(T content) where T : class => this.Action(x => x.Payloads.Add(PayloadBuilder.Create(content)));
    public NetResponseBuilder Add(string key, string value) => this.Action(x => x.Headers.Add(new KeyValuePair<string, string>(key, value)));

    public NetResponse Build()
    {
        Payloads.NotNull();

        return new NetResponse
        {
            StatusCode = StatusCode,
            Message = Message,
            Payloads = Payloads.ToArray(),
            Headers = Headers.ToArray(),
        };
    }
}
