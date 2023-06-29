using SpinCluster.sdk.Actors.Storage;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace SpinCluster.sdk.Actors.Storage;

public static class StorageBlobExtensions
{
    public static StorageBlob WithHash(this StorageBlob blob)
    {
        blob.NotNull();

        return blob with
        {
            HashBase64 = blob.ComputeHash(),
        };
    }

    public static string ComputeHash(this StorageBlob document) => new object?[]
    {
        document.ObjectId,
        document.TypeName,
        document.Content,
        document.Tags,
    }.ComputeHash()
    .Func(x => Convert.ToBase64String(x));

    public static bool IsHashVerify(this StorageBlob blob)
    {
        blob.NotNull();

        string hashBase64 = blob.ComputeHash();
        return blob.HashBase64 == hashBase64;
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
