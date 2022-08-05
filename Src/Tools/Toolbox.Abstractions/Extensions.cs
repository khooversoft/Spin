using System.Text.Json.Nodes;
using Toolbox.Actor;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Abstractions;

public static class Extensions
{
    public static ActorKey ToActorKey(this DocumentId documentId) => (ActorKey)documentId.NotNull().Id;

    public static DocumentId ToDocumentId(this ActorKey actorKey) => (DocumentId)actorKey.Value;

    public static Document WithHash(this Document document)
    {
        document.NotNull();

        return new Document
        {
            DocumentId = document.DocumentId,
            ObjectClass = document.ObjectClass,
            TypeName = document.TypeName,
            Data = document.Data,
            PrincipleId = document.PrincipleId,
            Hash = document.ComputeHash(),
        };
    }

    public static Document Verify(this Document document, bool principleRequired = false)
    {
        document.NotNull();
        document.IsHashVerify().Assert(x => x == true, "Document is not valid");
        document.ObjectClass.NotEmpty(name: $"{nameof(document.ObjectClass)} is required");

        if (principleRequired) document.PrincipleId.NotEmpty(name: $"{nameof(document.PrincipleId)} is required");

        return document;
    }

    public static byte[] ComputeHash(this Document document) => new string?[]
    {
        document.DocumentId,
        document.ObjectClass,
        document.TypeName,
        document.Data,
        document.PrincipleId
    }.ComputeHash();

    public static bool IsHashVerify(this Document document)
    {
        document.NotNull();

        byte[] hash = document.ComputeHash();
        return hash.SequenceEqual(document.Hash);
    }

    public static T ToObject<T>(this Document document)
    {
        return typeof(T) switch
        {
            Type type when type == typeof(string) => (T)(object)document.Data,
            _ => Json.Default.Deserialize<T>(document.Data)!
        };
    }

    public static string ToJson(this Document subject)
    {
        subject.Verify();

        var documentBase = subject.ConvertTo();
        var jsonObject = JsonObject.Parse(documentBase.ToJson()).NotNull();

        jsonObject[nameof(subject.Data)] = subject.TypeName switch
        {
            "String" => JsonValue.Create(subject.Data),
            _ => JsonObject.Parse(subject.Data),
        };

        return jsonObject.ToJsonString();
    }
}
