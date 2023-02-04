using SpinNet.sdk.Model;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace SpinNet.sdk.Application;

public class PayloadBuilder
{
    public string? PayloadId { get; set; }
    public string? TypeName { get; set; }
    public string? Data { get; set; }

    public PayloadBuilder SetPayloadId(string? value) => this.Action(x => x.PayloadId = value);
    public PayloadBuilder SetTypeName(string? value) => this.Action(x => x.TypeName = value);
    public PayloadBuilder SetData(string? value) => this.Action(x => x.Data = value);

    public PayloadBuilder SetData<T>(T value) where T : class
    {
        value.NotNull();

        TypeName = typeof(T).GetTypeName();
        Data = value?.ToJson();
        return this;
    }

    public Payload Build()
    {
        TypeName.NotEmpty(message: "Field is required");
        Data.NotEmpty(message: "Field is required");

        return new Payload
        {
            PayloadId = PayloadId ?? Guid.NewGuid().ToString(),
            TypeName = TypeName,
            Data = Data,
        };
    }
}