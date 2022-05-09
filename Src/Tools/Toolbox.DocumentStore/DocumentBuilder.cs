using System.Text;
using Toolbox.Abstractions;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.DocumentStore;

public class DocumentBuilder
{
    public DocumentBuilder()
    {
    }

    public DocumentBuilder(Document document)
    {
        document.NotNull(nameof(document));

        DocumentId = document.DocumentId.NotNull(nameof(document.DocumentId));
        document.Properties.ForEach(x => Properties.Add(x.Key, x.Value));
        ObjectClass = document.ObjectClass.NotEmpty(nameof(document.ObjectClass));
        Data = document.Data.NotNull(nameof(document.Data));
    }

    public DocumentId? DocumentId { get; set; }

    public IDictionary<string, string> Properties { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public string? ObjectClass { get; set; }

    public byte[]? Data { get; set; }

    public DocumentBuilder SetDocumentId(DocumentId document) => this.Action(x => x.DocumentId = document);

    public DocumentBuilder SetProperties(string key, string value) => this.Action(x => x.Properties[key] = value);

    public DocumentBuilder SetObjectClass(string objectClass) => this.Action(x => x.ObjectClass = objectClass);

    public DocumentBuilder SetData(byte[] data) => this.Action(x => x.Data = data);

    public DocumentBuilder SetData<T>(T value, string? objectClass = null)
    {
        value.NotNull(nameof(value));

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
        DocumentId.NotNull($"{nameof(DocumentId)} is required");
        Properties.NotNull($"{nameof(Properties)} is required");
        ObjectClass.NotEmpty($"{nameof(ObjectClass)} is required");
        Data.NotNull($"{nameof(Data)} is required");

        byte[] hash = DocumentTools.ComputeHash(DocumentId!, Properties!, ObjectClass!, Data!);

        return new Document
        {
            DocumentId = DocumentId,
            Properties = Properties.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase),
            ObjectClass = ObjectClass,
            Data = Data,
            Hash = hash
        };
    }
}
