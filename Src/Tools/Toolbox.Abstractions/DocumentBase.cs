using Toolbox.Tools;

namespace Toolbox.Abstractions;

public record DocumentBase
{
    public DocumentId DocumentId { get; init; } = null!;
    public string ObjectClass { get; init; } = null!;
    public string TypeName { get; init; } = null!;
    public byte[] Hash { get; init; } = null!;
    public string? PrincipleId { get; init; }
}


public static class DocumentBaseExtensions
{
    public static DocumentBase ConvertTo(this Document document)
    {
        document.NotNull();

        return new DocumentBase
        {
            DocumentId = (DocumentId)document.DocumentId,
            ObjectClass = document.ObjectClass,
            TypeName = document.TypeName,
            Hash = document.Hash,
            PrincipleId = document.PrincipleId,
        };
    }

    public static Document ConvertTo(this DocumentBase documentBase, string data)
    {
        documentBase.NotNull();
        data.NotEmpty();

        return new Document
        {
            DocumentId = (string)documentBase.DocumentId,
            ObjectClass = documentBase.ObjectClass,
            TypeName = documentBase.TypeName,
            Data = data,
            Hash = documentBase.Hash,
            PrincipleId = documentBase.PrincipleId,
        }.Verify();
    }
}