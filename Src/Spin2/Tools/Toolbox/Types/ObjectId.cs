using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tokenizer.Token;
using Toolbox.Tools;

namespace Toolbox.Types;

///   {schema}:tenant/path[/path...]
public sealed record ObjectId
{
    public string? _path;
    public string? _id;

    public ObjectId(string schema, string tentant, IEnumerable<string> paths)
    {
        Schema = schema;
        Tenant = tentant;
        Paths = paths.ToArray();
    }

    public const string Syntax = "{schema}/{tenant}[/{path}...] Valid characters are a-z A-Z 0-9 . $ @ - _ *";

    public string Schema { get; }
    public string Tenant { get; }
    public IReadOnlyList<string> Paths { get; }

    public string Path => _path ?? Paths.Join("/");
    public string Id => _id ?? $"{Schema}/{Tenant}/{Path}";
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

        var badResult = new Option<ObjectId>(StatusCode.BadRequest);

        if (tokenStack.Count < 2) return badResult;

        if (!tokenStack.TryPop(out string? schema)) return badResult;
        if (!Test(schema)) return badResult;

        if (!tokenStack.TryPop(out string? tenant)) return badResult;
        if (!Test(tenant)) return badResult;

        IReadOnlyList<string> paths = tokenStack.ToArray();
        if (!paths.All(x => Test(x))) return badResult;

        return new ObjectId(schema, tenant, paths);
    }

    private static bool Test(string subject) => subject
        .All(x => char.IsLetterOrDigit(x) || x == '.' || x == '-' || x == '$' || x == '@' || x == '_'  || x == '*');
}

public static class ObjectIdExtensions
{
    public static ObjectId ToObjectId(this string? subject) => ObjectId.Parse(subject).ThrowOnError().Return();
    public static Option<ObjectId> ToObjectIdIfValid(this string subject) => ObjectId.CreateIfValid(subject);
    public static string ToUrlEncoding(this ObjectId subject) => Uri.EscapeDataString(subject.ToString());
    public static ObjectId FromUrlEncoding(this string id) => Uri.UnescapeDataString(id).ToObjectId();

    public static string GetParent(this ObjectId uri) => uri.Paths
        .Take(Math.Max(1, uri.Paths.Count - 2))
        .Join("/");
}