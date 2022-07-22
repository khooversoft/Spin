using System.Security.Cryptography;
using System.Text;
using Toolbox.Abstractions;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.DocumentStore;

public static class DocumentTools
{
    public static Document Verify(this Document document)
    {
        document.IsHashVerify().Assert(x => x == true, "Document is not valid");
        document.ObjectClass.NotNull(name: $"{nameof(document.ObjectClass)} is required");

        return document;
    }

    public static byte[] ComputeHash(this DocumentId documentId, string objectClass, string data)
    {
        documentId.NotNull();
        objectClass.NotEmpty();
        data.NotNull();

        var ms = new MemoryStream();
        ms.Write(documentId.ToString().ToBytes());
        ms.Write(data.ToBytes());

        ms.Seek(0, SeekOrigin.Begin);
        return MD5.Create().ComputeHash(ms);
    }

    public static bool IsHashVerify(this Document document)
    {
        document.NotNull();

        byte[] hash = DocumentTools.ComputeHash(document.DocumentId, document.ObjectClass, document.Data);

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
}
