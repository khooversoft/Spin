using System.Collections;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.DocumentContainer;

public class DocumentBuilder
{
    public DocumentBuilder() { }

    public DocumentId? DocumentId { get; set; }
    public string? TypeName { get; set; }
    public string? Content { get; set; }
    public string? HashBase64 { get; set; }
    public string? Tags { get; set; }

    public DocumentBuilder SetDocumentId(DocumentId value) => this.Action(x => x.DocumentId = value);
    public DocumentBuilder SetHashBase64(string value) => this.Action(x => x.HashBase64 = value);
    public DocumentBuilder SetTags(params string[] value) => this.Action(x => x.Tags = value.Join(";"));

    public DocumentBuilder SetContent(string value)
    {
        Content = value;
        TypeName = typeof(string).GetTypeName();

        return this;
    }

    public DocumentBuilder SetContent<T>(T value) where T : class
    {
        const string errorMsg = "Unsupported type";
        value.NotNull();

        Content = typeof(T) switch
        {
            Type type when type == typeof(string) => value.ToString().NotEmpty(),
            Type type when type.IsAssignableTo(typeof(Array)) => throw new ArgumentException(errorMsg),
            Type type when type.IsAssignableTo(typeof(IEnumerable)) => throw new ArgumentException(errorMsg),

            _ => Json.Default.SerializeDefault(value),
        };

        TypeName = typeof(T).GetTypeName();
        return this;
    }

    public DocumentBuilder SetContent(byte[] bytes)
    {
        bytes.NotNull();

        TypeName = "bytes";
        Content = Convert.ToBase64String(bytes);
        return this;
    }

    public Document Build()
    {
        DocumentId.NotNull(name: "required");
        TypeName.NotEmpty(name: "required");
        Content.NotEmpty(name: "required");

        return new Document
        {
            DocumentId = (string)DocumentId,
            TypeName = TypeName,
            Content = Content,
        }.WithHash();
    }
}
