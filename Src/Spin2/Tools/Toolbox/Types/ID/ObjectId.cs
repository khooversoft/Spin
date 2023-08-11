using System.Text.Json.Serialization;
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

    [JsonConstructor]
    public ObjectId(string schema, string tenant, IReadOnlyList<string> paths)
        : this(schema, tenant, paths.ToArray())
    {
    }

    public ObjectId(string schema, string tenant, params string?[] paths)
    {
        Schema = schema.Assert(x => IdPatterns.IsSchema(x), _syntaxError);
        Tenant = tenant.Assert(x => IdPatterns.IsTenant(x), _syntaxError);

        Paths = (paths ?? Array.Empty<string>())
            .Where(x => x != null)
            .OfType<string>()
            .Join('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Action(x => x.ForEach(x => x.Assert(x => IdPatterns.IsPath(x), _syntaxError)));
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

    [JsonIgnore] public string Path => _path ?? Paths.Join("/");
    [JsonIgnore] public string Id => _id ?? $"{Schema}/{Tenant}/{Path}".RemoveTrailing('/');
    [JsonIgnore] public string FilePath => _filePath ?? Tenant + "/" + Path;

    public override string ToString() => Id;
    public bool Equals(ObjectId? obj) => obj is ObjectId value &&
        value.Schema == Schema &&
        value.Tenant == Tenant &&
        value.Paths.Count == Paths.Count &&
        Enumerable.SequenceEqual(Paths, value.Paths);

    public override int GetHashCode() => HashCode.Combine(Schema, Tenant, Path);

    public static implicit operator ObjectId(string subject) => Parse(subject).Return();
    public static implicit operator string(ObjectId subject) => subject.ToString();

    public static Option<ObjectId> Create(string id) => ObjectId.Parse(id);

    public static bool IsValid(string? id) => ObjectId.Parse(id).IsOk();

    private static Option<ObjectId> Parse(string? objectId)
    {
        if (objectId.IsEmpty()) return new Option<ObjectId>(StatusCode.BadRequest);

        Stack<string> tokenStack = Uri.UnescapeDataString(objectId)
            .RemoveTrailing('/')
            .Split('/')
            .Reverse()
            .ToStack();

        if (tokenStack.Count < 3) return new Option<ObjectId>(StatusCode.BadRequest, "Missing schema, tenant, path(s)");

        string schema = tokenStack.Pop();
        if (!IdPatterns.IsSchema(schema)) return new Option<ObjectId>(StatusCode.BadRequest, "Invalid characters");

        string tenant = tokenStack.Pop();
        if (!IdPatterns.IsTenant(tenant)) return new Option<ObjectId>(StatusCode.BadRequest, "Invalid characters");

        var paths = tokenStack.ToArray();
        if (!paths.All(x => IdPatterns.IsPath(x))) return new Option<ObjectId>(StatusCode.BadRequest, "Path(s) has invalid characters");

        return new ObjectId(schema, tenant, paths);
    }
}

public static class ObjectIdExtensions
{
    public static string ToUrlEncoding(this ObjectId subject) => Uri.EscapeDataString(subject.ToString());

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