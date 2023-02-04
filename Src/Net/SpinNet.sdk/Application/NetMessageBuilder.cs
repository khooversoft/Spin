using SpinNet.sdk.Model;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace SpinNet.sdk.Application;

public class NetMessageBuilder
{
    public string? MessageId { get; set; }
    public string? FromId { get; set; }
    public string? ToId { get; set; }
    public string? Command { get; set; }
    public IList<Payload> Payloads { get; } = new List<Payload>();
    public IList<ConfigEntry> Configuration { get; init; } = new List<ConfigEntry>();

    public NetMessageBuilder SetMessageId(string? value) => this.Action(x => x.MessageId = value);
    public NetMessageBuilder SetFromId(string? value) => this.Action(x => x.FromId = value);
    public NetMessageBuilder SetToId(string? value) => this.Action(x => x.ToId = value);
    public NetMessageBuilder SetCommand(string? value) => this.Action(x => x.Command = value);
    public NetMessageBuilder Add(Payload value) => this.Action(x => x.Payloads.Add(value.NotNull().Verify()));
    public NetMessageBuilder Add(ConfigEntry value) => this.Action(x => x.Configuration.Add(value));


    public NetMessage Build()
    {
        FromId.NotEmpty(message: "Field is required");
        ToId.NotEmpty(message: "Field is required");
        Command.NotEmpty(message: "Field is required");

        return new NetMessage
        {
            MessageId = MessageId ?? Guid.NewGuid().ToString(),
            FromId = FromId,
            ToId = ToId,
            Command = Command,
            Payloads = (Payloads ?? Enumerable.Empty<Payload>()).ToArray(),
            Configuration = (Configuration ?? Enumerable.Empty<ConfigEntry>()).ToArray(),
        };
    }
}
