using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

///   {schema}:tenant/path[/path...]
public sealed record ObjectId
{
    private const string _syntaxError = "Syntax error";
    private string? _path = null;
    private string? _id = null;
    private string? _filePath = null;

    public ObjectId(string schema, string tentant, params string?[] paths)
    {
        Schema = schema.Assert(x => IsNameValid(x), _syntaxError);
        Tenant = tentant.Assert(x => IsNameValid(x), _syntaxError);

        Paths = (paths ?? Array.Empty<string>())
            .Where(x => x != null)
            .OfType<string>()
            .Join('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Action(x => x.ForEach(x => x.Assert(x => IsNameValid(x), _syntaxError)));
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
    public string FilePath => _filePath ?? Tenant + "/" + Path;

    public override string ToString() => Id;
    public bool Equals(ObjectId? obj) => obj is ObjectId value &&
        value.Schema == Schema &&
        value.Tenant == Tenant &&
        value.Paths.Count == Paths.Count &&
        Enumerable.SequenceEqual(Paths, value.Paths);

    public override int GetHashCode() => HashCode.Combine(Schema, Tenant, Path);

    public static Option<ObjectId> CreateIfValid(string id)
    {
        Option<ObjectId> objectId = ObjectId.Parse(id);
        if (objectId.IsError()) return objectId.ToOption<ObjectId>();

        return objectId;
    }

    public static bool IsValid(string? id) => ObjectId.Parse(id).HasValue;

    public static Option<ObjectId> Parse(string? objectId)
    {
        if (objectId.IsEmpty()) return new Option<ObjectId>(StatusCode.BadRequest);

        Stack<string> tokenStack = objectId
            .RemoveTrailing('/')
            .Split('/')
            .Reverse()
            .ToStack();

        if (tokenStack.Count < 2) return new Option<ObjectId>(StatusCode.BadRequest, "Syntax error, no schema and tenant");

        string schema = tokenStack.Pop();
        if (!IsNameValid(schema)) return new Option<ObjectId>(StatusCode.BadRequest, "Syntax error, schema has invalid characters");

        string tenant = tokenStack.Pop();
        if (!IsNameValid(tenant)) return new Option<ObjectId>(StatusCode.BadRequest, "Syntax error, tenant has invalid characters");

        var paths = tokenStack.ToArray();
        if (!paths.All(x => IsNameValid(x))) return new Option<ObjectId>(StatusCode.BadRequest, "Syntax error, one or more of the paths has invalid characters");

        return new ObjectId(schema, tenant, paths);
    }

    public static bool IsNameValid(string subject) => subject.IsNotEmpty() && subject.All(x => IsCharacterValid(x));

    public static bool IsCharacterValid(char ch) =>
        char.IsLetterOrDigit(ch) || ch == '.' || ch == '-' || ch == '$' || ch == '@' || ch == '_' || ch == '*';
}

public static class ObjectIdExtensions
{
    public static ObjectId ToObjectId(this string? subject) => ObjectId.Parse(subject).Return(true);
    public static ObjectId ToObjectId(this string? subject, string schema, string tenant) => ObjectId.Parse($"{schema}/{tenant}/{subject}").Return(true);
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

    public static ObjectId WithSchema(this ObjectId objectId, string schema) => new ObjectId(schema.NotEmpty(), objectId.Tenant, objectId.Path);

    public static ObjectId WithExtension(this ObjectId objectId, string extension) => objectId switch
    {
        var v when v.Path.IsEmpty() => v,
        var v => new ObjectId(v.Schema, v.Tenant, PathTools.SetExtension(v.Path, extension)),
    };
}