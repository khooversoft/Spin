using SpinNet.sdk.Model;
using Toolbox.Extensions;
using Toolbox.Model;
using Toolbox.Tools;

namespace SpinNet.sdk.Application;

public class NetMessageBuilder
{
    public string? MessageId { get; set; }
    public string? ResourceUri { get; set; }
    public string? Command { get; set; }
    public IList<Payload> Payloads { get; } = new List<Payload>();
    public IList<KeyValuePair<string, string>> Headers { get; } = new List<KeyValuePair<string, string>>();

    public NetMessageBuilder SetMessageId(string? value) => this.Action(x => x.MessageId = value);
    public NetMessageBuilder SetResourceUri(string? value) => this.Action(x => x.ResourceUri = value);
    public NetMessageBuilder SetCommand(string? value) => this.Action(x => x.Command = value);
    public NetMessageBuilder Add(Payload value) => this.Action(x => x.Payloads.Add(value.NotNull().Verify()));
    public NetMessageBuilder Add<T>(T content) where T : class => this.Action(x => x.Payloads.Add(PayloadBuilder.Create(content)));
    public NetMessageBuilder Add(string key, string value) => this.Action(x => x.Headers.Add(new KeyValuePair<string, string>(key, value)));


    public NetMessage Build()
    {
        ResourceUri.NotEmpty(message: "Field is required");
        Command.NotEmpty(message: "Field is required");

        return new NetMessage
        {
            MessageId = MessageId ?? Guid.NewGuid().ToString(),
            ResourceUri = ResourceUri,
            Command = Command,
            Payloads = (Payloads ?? Enumerable.Empty<Payload>()).ToArray(),
            Headers = Headers.ToArray(),
        };
    }
}
