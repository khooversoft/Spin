using System.Text;
using Toolbox.Abstractions;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.DocumentStore;

public class DocumentBuilder
{
    public DocumentBuilder() { }

    public DocumentBuilder(Document document)
    {
        document.NotNull();

        DocumentId = document.DocumentId.NotNull();
        ObjectClass = document.ObjectClass.NotEmpty();
        Data = document.Data.NotNull();
    }

    public DocumentId? DocumentId { get; set; }

    public string? ObjectClass { get; set; }
    public byte[]? Data { get; set; }
    public string? PrincipleId { get; set; }

    public DocumentBuilder SetDocumentId(DocumentId document) => this.Action(x => x.DocumentId = document);
    public DocumentBuilder SetObjectClass(string objectClass) => this.Action(x => x.ObjectClass = objectClass);
    public DocumentBuilder SetPrincipleId(string principleId) => this.Action(x => x.PrincipleId = principleId);

    public DocumentBuilder SetData(byte[] data) => this.Action(x => x.Data = data);

    public DocumentBuilder SetData<T>(T value, string? objectClass = null)
    {
        value.NotNull();

        byte[] data = typeof(T) switch
        {
            Type type when type == typeof(string) => Encoding.UTF8.GetBytes((string)(object)value),

            _ => Encoding.UTF8.GetBytes(Json.Default.SerializeFormat<T>(value)),
        };

        ObjectClass = objectClass ?? typeof(T).Name;
        SetData(data);

        return this;
    }


    public Document Build()
    {
        DocumentId.NotNull(name: $"{nameof(DocumentId)} is required");
        ObjectClass.NotEmpty(name: $"{nameof(ObjectClass)} is required");
        Data.NotNull(name: $"{nameof(Data)} is required");

        byte[] hash = DocumentTools.ComputeHash(DocumentId!, ObjectClass!, Data!);

        return new Document
        {
            DocumentId = DocumentId,
            ObjectClass = ObjectClass,
            Data = Data,
            Hash = hash,
            PrincipleId = PrincipleId,
        };
    }
}
