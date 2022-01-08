using System.Text;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Document;

public class DocumentBuilder
{
    public DocumentBuilder()
    {
    }

    public DocumentBuilder(Document document)
    {
        document.VerifyNotNull(nameof(document));

        DocumentId = document.DocumentId.VerifyNotNull(nameof(document.DocumentId));
        document.Properties.ForEach(x => Properties.Add(x.Key, x.Value));
        ObjectClass = document.ObjectClass.VerifyNotEmpty(nameof(document.ObjectClass));
        Data = document.Data.VerifyNotNull(nameof(document.Data));
    }

    public DocumentId? DocumentId { get; set; }

    public IDictionary<string, string> Properties { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public string? ObjectClass { get; set; }

    public byte[]? Data { get; set; }

    public DocumentBuilder SetDocumentId(DocumentId document) => this.Action(x => x.DocumentId = document);

    public DocumentBuilder SetProperties(string key, string value) => this.Action(x => x.Properties[key] = value);

    public DocumentBuilder SetObjectClass(string objectClass) => this.Action(x => x.ObjectClass = objectClass);

    public DocumentBuilder SetData(byte[] data) => this.Action(x => x.Data = data);

    public DocumentBuilder SetData<T>(T value)
    {
        value.VerifyNotNull(nameof(value));

        byte[] data = typeof(T) switch
        {
            Type type when type == typeof(string) => Encoding.UTF8.GetBytes((string)(object)value),

            _ => Encoding.UTF8.GetBytes(Json.Default.SerializeFormat<T>(value)),
        };

        ObjectClass = typeof(T).Name;
        SetData(data);

        return this;
    }


    public Document Build()
    {
        DocumentId.VerifyNotNull($"{nameof(DocumentId)} is required");
        Properties.VerifyNotNull($"{nameof(Properties)} is required");
        ObjectClass.VerifyNotEmpty($"{nameof(ObjectClass)} is required");
        Data.VerifyNotNull($"{nameof(Data)} is required");

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
