using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Data;

public static class DocumentExtensions
{
    public static Document WithHash(this Document document)
    {
        document.NotNull();

        return document with
        {
            ETag = document.ComputeHash(),
        };
    }

    public static string ComputeHash(this Document document) => new object?[]
    {
        document.ObjectId,
        document.TypeName,
        document.Content,
        document.Tags,
    }.ComputeHash()
    .Func(x => Convert.ToBase64String(x));

    public static bool IsHashVerify(this Document document)
    {
        document.NotNull();

        string hashBase64 = document.ComputeHash();
        return document.ETag == hashBase64;
    }

    public static T ToObject<T>(this Document document)
    {
        return typeof(T) switch
        {
            Type type when type == typeof(string) => (T)(object)document.Content.BytesToString(),
            Type type when type == typeof(byte[]) => (T)(object)document.Content,

            _ => Json.Default.Deserialize<T>(document.Content.BytesToString()).NotNull(),
        };
    }
}