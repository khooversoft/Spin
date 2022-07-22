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
    public string? TypeClassName { get; set; }
    public string? Data { get; set; }
    public string? PrincipleId { get; set; }

    public DocumentBuilder SetDocumentId(DocumentId document) => this.Action(x => x.DocumentId = document);
    public DocumentBuilder SetObjectClass(string objectClass) => this.Action(x => x.ObjectClass = objectClass);
    public DocumentBuilder SetPrincipleId(string principleId) => this.Action(x => x.PrincipleId = principleId);

    public DocumentBuilder SetData<T>(T value) where T : class
    {
        value.NotNull();

        Data = typeof(T) switch
        {
            Type type when type == typeof(string) => value.ToString()!,
            _ => Json.Default.SerializeDefault<T>(value),
        };

        TypeClassName = typeof(T).Name;
        return this;
    }


    public Document Build()
    {
        const string classError = $"{nameof(ObjectClass)} || {nameof(TypeClassName)} is required";

        DocumentId.NotNull(name: $"{nameof(DocumentId)} is required");
        Data.NotEmpty(name: $"{nameof(Data)} is required");
        (!ObjectClass.IsEmpty() || !TypeClassName.IsEmpty()).Assert(x => x == true, classError);

        string objectClass = ObjectClass ?? TypeClassName ?? throw new ArgumentException(classError);

        byte[] hash = DocumentTools.ComputeHash(DocumentId!, objectClass!, Data!);

        return new Document
        {
            DocumentId = DocumentId,
            ObjectClass = objectClass,
            Data = Data,
            Hash = hash,
            PrincipleId = PrincipleId,
        };
    }
}
