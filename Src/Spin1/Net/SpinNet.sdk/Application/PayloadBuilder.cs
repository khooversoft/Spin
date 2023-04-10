using SpinNet.sdk.Model;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace SpinNet.sdk.Application;

public class PayloadBuilder
{
    public string? PayloadId { get; set; }
    public string? TypeName { get; set; }
    public string? Content { get; set; }

    public PayloadBuilder SetPayloadId(string? value) => this.Action(x => x.PayloadId = value);
    public PayloadBuilder SetTypeName(string? value) => this.Action(x => x.TypeName = value);
    public PayloadBuilder SetContent(string? value) => this.Action(x => x.Content = value);

    public PayloadBuilder SetContent<T>(T value) where T : class
    {
        value.NotNull();

        TypeName = typeof(T).GetTypeName();
        Content = value?.ToJson();
        return this;
    }

    public Payload Build()
    {
        TypeName.NotEmpty(message: "Field is required");
        Content.NotEmpty(message: "Field is required");

        return new Payload
        {
            PayloadId = PayloadId ?? Guid.NewGuid().ToString(),
            TypeName = TypeName,
            Content = Content,
        };
    }

    public static Payload Create<T>(T value) where T : class => new PayloadBuilder().SetContent(value).Build();
}