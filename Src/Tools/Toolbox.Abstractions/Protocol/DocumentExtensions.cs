using System.Text.Json.Nodes;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Protocol;

public static class DocumentExtensions
{
    public static Document WithHash(this Document document)
    {
        document.NotNull();

        return document with
        {
            HashBase64 = document.ComputeHash(),
        };
    }

    public static Document Verify(this Document document, bool principleRequired = false)
    {
        document.NotNull();
        document.IsHashVerify().Assert("Document is not valid");

        if (principleRequired) document.PrincipleId.NotEmpty(name: $"{nameof(document.PrincipleId)} is required");

        return document;
    }

    public static string ComputeHash(this Document document) => new string?[]
    {
        document.DocumentId,
        document.TypeName,
        document.Content,
        document.PrincipleId,
        document.Tags,
    }.ComputeHash()
    .Func(x => Convert.ToBase64String(x));


    public static bool IsHashVerify(this Document document)
    {
        document.NotNull();

        string hashBase64 = document.ComputeHash();
        return document.HashBase64 == hashBase64;
    }

    public static T ToObject<T>(this Document document)
    {
        return typeof(T) switch
        {
            Type type when type == typeof(string) => (T)(object)document.Content,
            _ => Json.Default.Deserialize<T>(document.Content)!
        };
    }

    public static string ToJson(this Document subject)
    {
        subject.Verify();

        var jsonObject = JsonNode.Parse(subject.ToJson()).NotNull();

        jsonObject[nameof(subject.Content)] = subject.TypeName switch
        {
            "String" => JsonValue.Create(subject.Content),
            _ => JsonNode.Parse(subject.Content),
        };

        return jsonObject.ToJsonString();
    }
}
