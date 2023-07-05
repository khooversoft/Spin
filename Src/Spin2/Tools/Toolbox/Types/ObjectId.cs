using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

///   {schema}:tenant/path[/path...]
public sealed record ObjectId
{
    private const string _syntaxError = "Syntax error";
    public string? _path;
    public string? _id;

    public ObjectId(string schema, string tentant, params string?[] paths)
    {
        Schema = schema.Assert(x => IsPathValid(x), _syntaxError);
        Tenant = tentant.Assert(x => IsPathValid(x), _syntaxError);

        Paths = (paths ?? Array.Empty<string>())
            .Where(x => x != null)
            .OfType<string>()
            .Join('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Action(x => x.ForEach(x => x.Assert(x => IsPathValid(x), _syntaxError)));
    }

    public const string Syntax = "{schema}/{tenant}[/{path}...] Valid characters are a-z A-Z 0-9 . $ @ - _ *";

    public void Deconstruct(out string Schema, out string Tenant, out string Path)
    {
        Schema = this.Schema;
        Tenant = this.Tenant;
        Path = this.Path;
    }

    public string Schema { get; }
    public string Tenant { get; }
    public IReadOnlyList<string> Paths { get; }

    public string Path => _path ?? Paths.Join("/");
    public string Id => _id ?? $"{Schema}/{Tenant}/{Path}".RemoveTrailing('/');
    public override string ToString() => Id;

    public bool Equals(ObjectId? obj) => obj is ObjectId value &&
        value.Schema == Schema &&
        value.Tenant == Tenant &&
        value.Paths.Count == Paths.Count &&
        Enumerable.SequenceEqual(Paths, value.Paths);

    public override int GetHashCode() => HashCode.Combine(Schema, Tenant, Path);

    public static bool IsValid(string? id) => ObjectId.Parse(id).HasValue;

    public static Option<ObjectId> CreateIfValid(string id)
    {
        Option<ObjectId> objectId = ObjectId.Parse(id);
        if (objectId.IsError()) return objectId.ToOption<ObjectId>();

        return new ObjectId(objectId.Return());
    }

    public static Option<ObjectId> Parse(string? objectId)
    {
        if (objectId.IsEmpty()) return new Option<ObjectId>(StatusCode.BadRequest);

        Stack<string> tokenStack = objectId
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Reverse()
            .ToStack();

        if (tokenStack.Count < 2) return new Option<ObjectId>(StatusCode.BadRequest, "Syntax error, no schema and tenant");

        string schema = tokenStack.Pop();
        if (!IsPathValid(schema)) return new Option<ObjectId>(StatusCode.BadRequest, "Syntax error, schema has invalid characters");

        string tenant = tokenStack.Pop();
        if (!IsPathValid(tenant)) return new Option<ObjectId>(StatusCode.BadRequest, "Syntax error, tenant has invalid characters");

        var paths = tokenStack.ToArray();
        if (!paths.All(x => IsPathValid(x))) return new Option<ObjectId>(StatusCode.BadRequest, "Syntax error, one or more of the paths has invalid characters");

        return new ObjectId(schema, tenant, paths);
    }

    public static bool IsPathValid(string subject) => subject.All(x => IsCharacterValid(x));

    public static bool IsCharacterValid(char ch) =>
        char.IsLetterOrDigit(ch) || ch == '.' || ch == '-' || ch == '$' || ch == '@' || ch == '_' || ch == '*';
}

public static class ObjectIdExtensions
{
    public static ObjectId ToObjectId(this string? subject) => ObjectId.Parse(subject).ThrowOnError().Return();
    public static ObjectId ToObjectId(this string? subject, string schema, string tenant) => ObjectId.Parse($"{schema}/{tenant}/{subject}").ThrowOnError().Return();
    public static Option<ObjectId> ToObjectIdIfValid(this string subject) => ObjectId.CreateIfValid(subject);

    public static Option<ObjectId> ToObjectIdIfValid(this string subject, ScopeContextLocation location)
    {
        var option = ObjectId.CreateIfValid(subject);
        if (option.IsError()) location.LogError("ObjectId is not valid, objectId={objectId}, error={error}", subject, option.Error);
        return option;
    }


    public static string ToUrlEncoding(this ObjectId subject) => Uri.EscapeDataString(subject.ToString());
    public static ObjectId FromUrlEncoding(this string id) => Uri.UnescapeDataString(id).ToObjectId();

    public static string GetParent(this ObjectId subject) => subject.Paths
        .Take(Math.Max(0, subject.Paths.Count - 2))
        .Join("/");
}