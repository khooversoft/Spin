using System.Text.Json.Nodes;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.DocumentContainer;

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

    public static string ComputeHash(this Document document) => new string?[]
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

    public static T ToObject<T>(this Document document) => typeof(T) switch
    {
        Type type when type == typeof(string) => (T)(object)document.Content,
        _ => Json.Default.Deserialize<T>(document.Content)!
    };

    public static byte[] ToBytes(this Document subject)
    {
        subject.NotNull();

        string subjectJson = subject.ToJson();

        string json = subject.TypeName switch
        {
            string v when typeof(string).Name == v => subjectJson,
            _ => expand(),
        };

        return json.ToBytes();

        string expand()
        {
            string nodeJson = subject.Content;
            return Json.ExpandNode(subjectJson, "content", nodeJson);
        }
    }

    public static Document ToDocument(this byte[] data)
    {
        const string nodeName = "typeName";
        const string error = "serialization error";
        data.NotNull();

        string json = data.BytesToString();

        JsonObject sourceJsonObject = JsonNode.Parse(json).NotNull().AsObject();
        if (!sourceJsonObject.TryGetPropertyValue(nodeName, out var node)) throw new ArgumentException($"Cannot find nodeName={nodeName}");

        string typeName = node.NotNull().ToJsonString(Json.JsonSerializerOptions).NotEmpty() switch
        {
            string v when v.Length >= 2 && v[0] == '"' && v[^1] == '"' => v[1..^1],
            string v => v,
        };

        Document result = typeName switch
        {
            string v when typeof(string).Name == v => json.ToObject<Document>().NotNull(name: error),
            _ => Json.WrapNode(json, "content").ToObject<Document>().NotNull(name: error),
        };

        return result;
    }
}