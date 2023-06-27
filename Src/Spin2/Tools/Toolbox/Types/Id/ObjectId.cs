using Toolbox.Extensions;
using Toolbox.Tokenizer;
using Toolbox.Tokenizer.Token;
using Toolbox.Tools;
using Toolbox.Types.Id;

namespace Toolbox.Types;

/// <summary>
/// Object ID
/// 
///   {schema}/{tenant}/{path}[/path...]
///   
/// Valid characters are a-z A-Z 0-9 . $ @ -
///   
/// Examples of schema are domains like "contract", "file", "user", "group", etc...
/// 
/// Schema is required
/// Tenant is option, if not specified its system
/// Path is required
///   
/// </summary>
public sealed record ObjectId : ObjectIdBase
{
    private ObjectId(ParsedObjectId id) : base(id) { }

    public ObjectId(string id)
        : base(id, true, Syntax)
    {
    }

    public const string Syntax = "{schema}/{tenant}/{path}[/{path}...] Valid characters are a-z A-Z 0-9 . $ @ - _";

    public override string ToString() => Id;
    public bool Equals(ObjectId? other) => other is not null && Id == other.Id;
    public override int GetHashCode() => HashCode.Combine(Id);

    public static implicit operator ObjectId(string id) => new ObjectId(id);
    public static implicit operator string(ObjectId id) => id.ToString();

    public static Option<ObjectId> CreateIfValid(string id)
    {
        Option<ParsedObjectId> objectId = ParsedObjectId.Parse(id, true);
        if (objectId.IsError()) return objectId.ToOption<ObjectId>();

        return new ObjectId(objectId.Return());
    }
}

public static class ObjectIdExtensions
{
    public static ObjectId ToObjectId(this string subject) => (ObjectId)subject;
    public static Option<ObjectId> ToObjectIdIfValid(this string subject) => ObjectId.CreateIfValid(subject);
    public static string ToUrlEncoding(this ObjectId subject) => Uri.EscapeDataString((string)subject);
    public static ObjectId FromUrlEncoding(this string id) => Uri.UnescapeDataString(id).ToObjectId();
}