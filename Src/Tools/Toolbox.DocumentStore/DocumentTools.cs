//using System.Security.Cryptography;
//using System.Text;
//using System.Text.Json.Nodes;
//using Toolbox.Abstractions;
//using Toolbox.Extensions;
//using Toolbox.Tools;

//namespace Toolbox.DocumentStore;

//public static class DocumentTools
//{
//    public static Document Verify(this Document document)
//    {
//        document.IsHashVerify().Assert(x => x == true, "Document is not valid");
//        document.ObjectClass.NotNull(name: $"{nameof(document.ObjectClass)} is required");

//        return document;
//    }

//    public static byte[] ComputeHash(this Document document) =>
//        ComputeHash(document.DocumentId, document.ObjectClass, document.TypeName, document.Data, document.PrincipleId);

//    public static byte[] ComputeHash(this DocumentId documentId, params string?[] values)
//    {
//        documentId.NotNull();
//        values.NotNull();

//        var ms = new MemoryStream();
//        ms.Write(documentId.ToString().ToBytes());
//        values.Where(x => x != null).ForEach(x => ms.Write(x.ToBytes()));

//        ms.Seek(0, SeekOrigin.Begin);
//        return MD5.Create().ComputeHash(ms);
//    }

//    public static bool IsHashVerify(this Document document)
//    {
//        document.NotNull();

//        byte[] hash = document.ComputeHash();
//        return hash.SequenceEqual(document.Hash);
//    }

//    public static T ToObject<T>(this Document document)
//    {
//        return typeof(T) switch
//        {
//            Type type when type == typeof(string) => (T)(object)document.Data,
//            _ => Json.Default.Deserialize<T>(document.Data)!
//        };
//    }

//    public static string ToJson(this Document subject)
//    {
//        subject.Verify();

//        var documentBase = subject.ConvertTo();
//        var jsonObject = JsonObject.Parse(documentBase.ToJson()).NotNull();

//        jsonObject[nameof(subject.Data)] = subject.TypeName switch
//        {
//            "String" => JsonValue.Create(subject.Data),
//            _ => JsonObject.Parse(subject.Data),
//        };

//        return jsonObject.ToJsonString();
//    }
//}
