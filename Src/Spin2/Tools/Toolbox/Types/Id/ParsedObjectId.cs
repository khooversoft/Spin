using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tokenizer.Token;

namespace Toolbox.Types;

///   {schema}:tenant/path[/path...]
public sealed record ParsedObjectId
{
    public ParsedObjectId(string schema, string tentant, IEnumerable<string> paths)
    {
        Schema = schema;
        Tentant = tentant;
        Paths = paths.ToArray();
    }

    public const string Syntax = "{schema}/{tenant}[/{path}...] Valid characters are a-z A-Z 0-9 . $ @ - _ *";

    public string Schema { get; }
    public string Tentant { get; }
    public IReadOnlyList<string> Paths { get; }

    public string GetPath() => Paths.Join("/");
    public override string ToString() => $"{Schema}/{Tentant}/" + Paths.Join("/");

    public bool Equals(ParsedObjectId? obj) => obj is ParsedObjectId value &&
        value.Schema == Schema &&
        value.Tentant == Tentant &&
        value.Paths.Count == Paths.Count &&
        Enumerable.SequenceEqual(Paths, value.Paths);

    public override int GetHashCode() => HashCode.Combine(Schema, Tentant, GetPath());

    public static implicit operator ParsedObjectId(string id) => Parse(id).Return();
    public static implicit operator string(ParsedObjectId id) => id.ToString();

    public static Option<ObjectId> CreateIfValid(string id)
    {
        Option<ParsedObjectId> objectId = ParsedObjectId.Parse(id);
        if (objectId.IsError()) return objectId.ToOption<ObjectId>();

        return new ObjectId(objectId.Return());
    }


    public static Option<ParsedObjectId> Parse(string? objectId)
    {
        if (objectId.IsEmpty()) return new Option<ParsedObjectId>(StatusCode.BadRequest);

        Stack<string> tokenStack = objectId
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Reverse()
            .ToStack();

        var badResult = new Option<ParsedObjectId>(StatusCode.BadRequest);

        if (tokenStack.Count < 3) return badResult;

        if (!tokenStack.TryPop(out string? schema)) return badResult;
        if (!Test(schema)) return badResult;

        if (!tokenStack.TryPop(out string? tenant)) return badResult;
        if (!Test(tenant)) return badResult;

        IReadOnlyList<string> paths = tokenStack.ToArray();
        if (!paths.All(x => Test(x))) return badResult;

        return new ParsedObjectId(schema, tenant, paths);
    }

    private static bool Test(string subject) => subject
        .All(x => char.IsLetterOrDigit(x) || x == '.' || x == '-' || x == '$' || x == '@' || x == '_'  || x == '*');
}

public static class ObjectIdExtensions
{
    public static ObjectId ToObjectId(this string subject) => (ObjectId)subject;
    public static Option<ObjectId> ToObjectIdIfValid(this string subject) => ObjectId.CreateIfValid(subject);
    public static string ToUrlEncoding(this ObjectId subject) => Uri.EscapeDataString((string)subject);
    public static ObjectId FromUrlEncoding(this string id) => Uri.UnescapeDataString(id).ToObjectId();
}