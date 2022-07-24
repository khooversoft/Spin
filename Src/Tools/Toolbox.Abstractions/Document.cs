using System.Text.Json.Nodes;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Abstractions;

public sealed record Document
{
    public string DocumentId { get; init; } = null!;
    public string ObjectClass { get; init; } = null!;
    public string TypeName { get; init; } = null!;
    public string Data { get; init; } = null!;
    public byte[] Hash { get; init; } = null!;
    public string? PrincipleId { get; init; }

    public bool Equals(Document? obj)
    {
        return obj is Document document &&
               DocumentId == document.DocumentId &&
               ObjectClass == document.ObjectClass &&
               TypeName == document.TypeName &&
               Data == document.Data &&
               Enumerable.SequenceEqual(Hash, document.Hash) &&
               PrincipleId == document.PrincipleId;
    }

    public override int GetHashCode() => HashCode.Combine(DocumentId, ObjectClass, TypeName, Data, Hash, PrincipleId);

    public static Document CreateFromJson(string json)
    {
        json.NotEmpty();

        DocumentBase documentBase = Json.Default.Deserialize<DocumentBase>(json).NotNull();
        JsonNode jsonObject = JsonObject.Parse(json).NotNull();

        string data = documentBase.TypeName switch
        {
            "String" => jsonObject[nameof(Data)].NotNull().ToString(),
            _ => jsonObject[nameof(Data)].NotNull().ToJsonString(),
        };

        return documentBase.ConvertTo(data);
    }
}
