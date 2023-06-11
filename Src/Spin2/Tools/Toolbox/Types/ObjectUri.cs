using System;
using System.Linq;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

/// <summary>
/// Id is a path of {domain}[/{resource}]
/// </summary>
public sealed record ObjectUri
{
    private string? _domain;
    private string? _path;
    private IReadOnlyList<string>? _vectors;

    public ObjectUri(string id)
    {
        id.NotEmpty();

        Id = id;
        IsValid(Id).Assert($"Syntax error, {Syntax}");
    }

    public void Deconstruct(out string Domain, out string? Path)
    {
        Domain = this.Domain;
        Path = this.Path;
    }

    public const string Syntax = "{domain}[/{path}...] - domain & path must be alpha numeric or '-' except at the beginning or trailing '-'";

    public string Id { get; }

    public string Domain => _domain ??= Id.Split('/')[0];

    public string? Path => _path ??= Id.Split('/') switch
    {
        var v when v.Length == 0 => null,
        var v => v.Skip(1).Join("/").ToNullIfEmpty(),
    };

    public IReadOnlyList<string> Vectors => _vectors ??= Id?.Split('/') ?? Array.Empty<string>();

    public override string ToString() => Id;

    public bool Equals(ObjectId? other) => other is not null && Id == other.Id;

    public override int GetHashCode() => HashCode.Combine(Id);

    public static implicit operator ObjectUri(string documentId) => new ObjectUri(documentId);

    public static implicit operator string(ObjectUri documentId) => documentId.ToString();

    public static bool IsValid(string? objectUri)
    {
        if (objectUri.IsEmpty()) return false;
        if (objectUri.StartsWith('.')) return false;
        if (objectUri.Length > 0 && char.IsNumber(objectUri[0])) return false;

        string[] parts = objectUri.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return parts.All(x => testPart(x));

        static bool testPart(string part)
        {
            if (part.EndsWith('.')) return false;
            if (part.EndsWith('-')) return false;

            return part.All(x => char.IsLetterOrDigit(x) || x == '.' || x == '-');
        }
    }
}

public static class ObjectIUExtensions
{
    public static ObjectUri ToObjectUri(this string? subject) => (ObjectUri)subject!;

    public static ObjectUri SetDomain(this ObjectUri objectUri, string domain) => objectUri.Vectors
        .Prepend(domain)
        .Join("/")
        .ToObjectUri();

    public static ObjectUri WithDomain(this ObjectUri objectUri, string domain) => objectUri.Vectors
        .Skip(1)
        .Prepend(domain)
        .Join("/")
        .ToObjectUri();

    public static string? GetFolder(this ObjectUri uri) => uri.Vectors
        .Skip(1)
        .Take(Math.Max(0, uri.Vectors.Count - 2))
        .Join("/").ToNullIfEmpty();

    public static string GetFolderAndDomain(this ObjectUri uri) => uri.Domain + "/" + GetFolder(uri);

    public static string? GetFile(this ObjectUri uri) => uri.Vectors
        .Skip(1)
        .TakeLast(1)
        .FirstOrDefault();

    public static string GetParent(this ObjectUri uri) => uri.Vectors
            .Take(Math.Max(1, uri.Vectors.Count - 2))
            .Join("/");
}