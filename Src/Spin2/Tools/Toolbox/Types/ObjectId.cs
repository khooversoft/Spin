using Toolbox.Extensions;
using Toolbox.Tokenizer;
using Toolbox.Tokenizer.Token;
using Toolbox.Tools;

namespace Toolbox.Types;

/// <summary>
/// Object ID
/// 
///   {schema}:path[/path...]
///   {schema}:@tenant/path[/path...]
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

    public const string Syntax = "{schema}:path[/path...] || {schema}:@tenant/path[/path...] valid characters: [azAZ09].-$";

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

    ///   {schema}:path[/path...]
    ///   {schema}://tenant/path[/path...]
    private static Option<ParsedObjectId> Parse(string? objectId)
    {
        if (objectId.IsEmpty()) return new Option<ParsedObjectId>(StatusCode.BadRequest);

        Stack<TokenValue> tokenStack = new StringTokenizer()
            .UseCollapseWhitespace()
            .Add(":", "@", "/")
            .Parse(objectId)
            .OfType<TokenValue>()
            .Reverse()
            .ToStack();

        var badResult = new Option<ParsedObjectId>(StatusCode.BadRequest);

        if (!tokenStack.TryPop(out TokenValue schema)) return badResult;
        if (!test(schema)) return badResult;

        if (!tokenStack.TryPop(out TokenValue colonToken) || colonToken != ":") return badResult;

        string? tenant = null;
        if (!tokenStack.TryPeek(out TokenValue slashToken)) return badResult;
        if (slashToken == "@")
        {
            tokenStack.Pop();
            if (!tokenStack.TryPop(out TokenValue tenantToken)) return badResult;
            if (!test(tenantToken)) return badResult;
            tenant = tenantToken;
        }

        IReadOnlyList<string> paths = tokenStack
            .Where(x => x != "/")
            .Select(x => (string)x)
            .ToArray();

        if (paths.Count == 0 || !paths.All(x => test(x))) return badResult;

        ParsedObjectId result = new ParsedObjectId(schema, tenant, paths);

        return new Option<ParsedObjectId>(result);


        static bool test(string subject) => subject
            .All(x => char.IsLetterOrDigit(x) || x == '.' || x == '-' || x == '$');
    }

    private readonly record struct ParsedObjectId
    {
        public ParsedObjectId(string schema, string? tentant, IReadOnlyList<string> paths)
        {
            this.Schema = schema;
            this.Tentant = tentant;
            this.Paths = paths;
        }

        public string Schema { get; }
        public string? Tentant { get; }
        public IReadOnlyList<string> Paths { get; }

        public override string ToString() => Schema + ":" + (Tentant != null ? $"@{Tentant}/" : string.Empty) + Paths.Join("/");
        public string GetPath() => Paths.Join("/");
    }
}

public static class ObjectIdExtensions
{
    public static ObjectId ToObjectId(this string subject) => (ObjectId)subject;
    public static string ToUrlEncoding(this ObjectId subject) => Uri.EscapeDataString((string)subject);
    public static ObjectId FromUrlEncoding(this string id) => Uri.UnescapeDataString(id).ToObjectId();
}