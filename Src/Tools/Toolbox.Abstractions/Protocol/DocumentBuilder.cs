using System.Collections;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Protocol;

public class DocumentBuilder
{
    public DocumentBuilder() { }

    public DocumentBuilder(Document document)
    {
        document.NotNull();

        DocumentId = (DocumentId)document.DocumentId.NotNull();
        ObjectClass = document.ObjectClass.NotEmpty();
        Data = document.Data.NotNull();
    }

    public DocumentId? DocumentId { get; private set; }

    public string? ObjectClass { get; private set; }
    public string? TypeName { get; private set; }
    public string? Data { get; private set; }
    public string? PrincipleId { get; private set; }

    public DocumentBuilder SetDocumentId(DocumentId document) => this.Action(x => x.DocumentId = document);
    public DocumentBuilder SetObjectClass(string objectClass) => this.Action(x => x.ObjectClass = objectClass);
    public DocumentBuilder SetPrincipleId(string principleId) => this.Action(x => x.PrincipleId = principleId);

    public DocumentBuilder SetData<T>(T value) where T : class
    {
        const string errorMsg = "Unsupported type";
        value.NotNull();

        Data = typeof(T) switch
        {
            Type type when type == typeof(string) => value.ToString().NotEmpty(),
            Type type when type.IsAssignableTo(typeof(Array)) => throw new ArgumentException(errorMsg),
            Type type when type.IsAssignableTo(typeof(IEnumerable)) => throw new ArgumentException(errorMsg),

            _ => Json.Default.SerializeDefault(value),
        };

        TypeName = typeof(T).GetTypeName();
        return this;
    }


    public Document Build()
    {
        DocumentId.NotNull(name: "required");
        TypeName.NotEmpty(name: "required");
        Data.NotEmpty(name: "required");

        return new Document
        {
            DocumentId = (string)DocumentId,
            ObjectClass = ObjectClass ?? TypeName,
            TypeName = TypeName,
            Data = Data,
            PrincipleId = PrincipleId,
        }.WithHash();
    }
}
