using System.Text.RegularExpressions;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

/// <summary>
/// Object ID, domain + path
/// </summary>
public sealed record ObjectId
{
    private string? _domain;
    private string? _path;
    private IReadOnlyList<string>? _vectors;

    public ObjectId(string id)
    {
        Id = id.NotEmpty();
        IsValid(id).Assert($"Syntax error, {Syntax}");
    }

    public void Deconstruct(out string Domain, out string Path)
    {
        Domain = this.Domain;
        Path = this.Path;
    }

    public const string Syntax = "{domain}:{path}[/{path}...]";

    public string Id { get; }

    public string Domain => _domain ??= Id.Split(':')[0];
    public string Path => _path ??= Id.Split(':')[1].Split('/', StringSplitOptions.RemoveEmptyEntries).Join('/');
    public (string Domain, string Path) Components => (Domain, Path);
    public IReadOnlyList<string> Vectors => _vectors ??= Path.Split('/');

    public override string ToString() => Id;

    public bool Equals(ObjectId? other) => other is not null && Id == other.Id;

    public override int GetHashCode() => HashCode.Combine(Id);

    public static implicit operator ObjectId(string documentId) => new ObjectId(documentId);

    public static implicit operator string(ObjectId documentId) => documentId.ToString();


    // Rules
    //  domain is required
    //  domain and path(s) can have alpha, numeric, '$', '.', '-'
    //  1 path is required
    public static bool IsValid(string? objectId)
    {
        if (objectId.IsEmpty()) return false;

        string[] domainParts = objectId.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (domainParts.Length != 2) return false;
        if (!test(domainParts[0])) return false;

        string[] parts = domainParts[^1].Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return false;
        return parts.All(x => test(x));

        static bool test(string subject) => subject
            .All(x => char.IsLetterOrDigit(x) || x == '.' || x == '-' || x == '$');
    }
}

public static class ObjectIdExtensions
{
    public static ObjectId ToObjectId(this string subject) => (ObjectId)subject;
    public static string ToUrlEncoding(this ObjectId subject) => Uri.EscapeDataString((string)subject);
    public static ObjectId FromUrlEncoding(this string id) => Uri.UnescapeDataString(id).ToObjectId();
}