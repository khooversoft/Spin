using System.Collections;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class DocumentBuilder
{
    public DocumentBuilder() { }

    public ObjectId? DocumentId { get; set; }
    public string? TypeName { get; set; }
    public byte[]? Content { get; set; }
    public string? HashBase64 { get; set; }
    public string? Tags { get; set; }

    public DocumentBuilder SetDocumentId(ObjectId value) => this.Action(x => x.DocumentId = value);
    public DocumentBuilder SetHashBase64(string value) => this.Action(x => x.HashBase64 = value);
    public DocumentBuilder SetTags(params string[] value) => this.Action(x => x.Tags = value.Join(";"));

    public DocumentBuilder SetContent<T>(T value) where T : class
    {
        const string errorMsg = "Unsupported type";
        value.NotNull();

        Content = typeof(T) switch
        {
            Type type when type == typeof(string) => value.ToString().NotEmpty().ToBytes(),
            Type type when type == typeof(byte[]) => (byte[])(object)value,
            Type type when type.IsAssignableTo(typeof(Array)) => throw new ArgumentException(errorMsg),
            Type type when type.IsAssignableTo(typeof(IEnumerable)) => throw new ArgumentException(errorMsg),

            _ => Json.Default.SerializeDefault(value).ToBytes(),
        };

        TypeName = typeof(T).GetTypeName();
        return this;
    }

    public Document Build()
    {
        const string msg = "required";
        DocumentId.NotNull(name: msg);
        TypeName.NotEmpty(name: msg);
        Content.NotNull(name: msg);

        return new Document
        {
            ObjectId = (string)DocumentId,
            TypeName = TypeName,
            Content = Content,
        }.WithHash();
    }
}
