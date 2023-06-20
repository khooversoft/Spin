using Toolbox.Extensions;
using Toolbox.Tokenizer;
using Toolbox.Tokenizer.Token;
using Toolbox.Tools;

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
public sealed record ObjectId
{
    private readonly ParsedObjectId _parsedObjectId;
    private readonly string? _id = null;
    private readonly string? _path = null;

    public ObjectId(string id)
    {
        Option<ParsedObjectId> option = Parse(id);
        option.IsOk().Assert($"Syntax error, {Syntax}");

        _parsedObjectId = option.Return();
    }

    public void Deconstruct(out string Schema, out string? Tenant, out string Path)
    {
        Schema = this.Schema;
        Tenant = this.Schema;
        Path = this.Path;
    }

    public const string Syntax = "{schema}/{tenant}/{path}[/{path}...] Valid characters are a-z A-Z 0-9 . $ @ - _";

    public string Id => _id ?? _parsedObjectId.ToString();
    public string Schema => _parsedObjectId.Schema;
    public string? Tenant => _parsedObjectId.Tentant;
    public string Path => _path ?? _parsedObjectId.GetPath();
    public IReadOnlyList<string> Paths => _parsedObjectId.Paths;

    public override string ToString() => Id;

    public bool Equals(ObjectId? other) => other is not null && Id == other.Id;

    public override int GetHashCode() => HashCode.Combine(Id);

    public static implicit operator ObjectId(string id) => new ObjectId(id);

    public static implicit operator string(ObjectId id) => id.ToString();

    public static bool IsValid(string objectId) => Parse(objectId).IsOk();

    ///   {schema}:tenant/path[/path...]
    private static Option<ParsedObjectId> Parse(string? objectId)
    {
        if (objectId.IsEmpty()) return new Option<ParsedObjectId>(StatusCode.BadRequest);

        Stack<string> tokenStack = objectId
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Reverse()
            .ToStack();

        var badResult = new Option<ParsedObjectId>(StatusCode.BadRequest);

        if( tokenStack.Count < 3) return badResult;

        if (!tokenStack.TryPop(out string? schema)) return badResult;
        if (!test(schema)) return badResult;

        if (!tokenStack.TryPop(out string? tenant)) return badResult;
        if (!test(tenant)) return badResult;

        IReadOnlyList<string> paths = tokenStack.ToArray();
        if (paths.Count == 0 || !paths.All(x => test(x))) return badResult;

        ParsedObjectId result = new ParsedObjectId(schema, tenant, paths);

        return new Option<ParsedObjectId>(result);


        static bool test(string subject) => subject
            .All(x => char.IsLetterOrDigit(x) || x == '.' || x == '-' || x == '$' || x == '@' || x == '_');
    }

    private readonly record struct ParsedObjectId
    {
        public ParsedObjectId(string schema, string tentant, IReadOnlyList<string> paths)
        {
            this.Schema = schema;
            this.Tentant = tentant;
            this.Paths = paths;
        }

        public string Schema { get; }
        public string Tentant { get; }
        public IReadOnlyList<string> Paths { get; }

        public override string ToString() => $"{Schema}/{Tentant}/" + Paths.Join("/");
        public string GetPath() => Paths.Join("/");
    }
}

public static class ObjectIdExtensions
{
    public static ObjectId ToObjectId(this string subject) => (ObjectId)subject;
    public static string ToUrlEncoding(this ObjectId subject) => Uri.EscapeDataString((string)subject);
    public static ObjectId FromUrlEncoding(this string id) => Uri.UnescapeDataString(id).ToObjectId();
}