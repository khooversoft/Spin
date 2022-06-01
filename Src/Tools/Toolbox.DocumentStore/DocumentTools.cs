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
        return document;
    }

    public static byte[] ComputeHash(this DocumentId documentId, IEnumerable<KeyValuePair<string, string>> properties, string objectClass, byte[] data)
    {
        documentId.NotNull();
        properties.NotNull();
        objectClass.NotEmpty();
        data.NotNull();

        var ms = new MemoryStream();
        ms.Write(documentId.ToString().ToBytes());

        properties.ForEach(x =>
        {
            ms.Write(x.Key.ToBytes());
            ms.Write(x.Value.ToBytes());
        });

        ms.Write(data);

        ms.Seek(0, SeekOrigin.Begin);
        return MD5.Create().ComputeHash(ms);
    }

    public static bool IsHashVerify(this Document document)
    {
        document.NotNull();

        byte[] hash = DocumentTools.ComputeHash(document.DocumentId, document.Properties, document.ObjectClass, document.Data);

        return hash.SequenceEqual(document.Hash);
    }

    public static T DeserializeData<T>(this Document document)
    {
        string strData = Encoding.UTF8.GetString(document.Data);

        return typeof(T) switch
        {
            Type type when type == typeof(string) => (T)(object)strData,

            _ => Json.Default.Deserialize<T>(strData)!
        };
    }
}
