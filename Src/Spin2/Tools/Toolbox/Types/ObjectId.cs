using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Toolbox.Application;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

/// <summary>
/// Id is a path of {domain}/{resource}
/// </summary>
public sealed record ObjectId
{
    //private const string _regexDomainNotRequired = "^([a-zA-Z][a-zA-Z0-9]*[:])?[a-zA-Z][a-zA-Z0-9]+([/][a-zA-Z][a-zA-Z0-9]*)*$";
    private const string _regexRequired = "^([a-zA-Z][a-zA-Z0-9]*[:]){1}[a-zA-Z][a-zA-Z0-9]*([/][a-zA-Z][a-zA-Z0-9]*)*$";
    private string? _domain;
    private string? _path;
    private IReadOnlyList<string>? _vectors;

    public ObjectId(string id)
    {
        id.NotEmpty(name: id);

        Id = id.ToLower();
        IsObjectIdValid(id).Assert(x => x.IsOk(), $"Syntax error, {Syntax}");
    }

    public void Deconstruct(out string Domain, out string Path)
    {
        Domain = this.Domain;
        Path = this.Path;
    }

    public const string Syntax = "{domain}:{path}[/{path}...]";

    public string Id { get; }

    public string Domain => _domain ??= Id.Split(':')[0];
    public string Path => _path ??= Id.Split(':')[1];
    public (string Domain, string Path) Components => (Domain, Path);
    public IReadOnlyList<string> Vectors => _vectors ??= Path.Split('/');

    public override string ToString() => Id;

    public bool Equals(ObjectId? other) => other is not null && Id == other.Id;

    public override int GetHashCode() => HashCode.Combine(Id);

    public static implicit operator ObjectId(string documentId) => new ObjectId(documentId);

    public static implicit operator string(ObjectId documentId) => documentId.ToString();

    public static StatusCode IsObjectIdValid(string objectId)
    {
        if (objectId.IsEmpty()) return StatusCode.BadRequest;

        return Regex.Match(objectId, _regexRequired, RegexOptions.IgnoreCase).Success switch
        {
            true => StatusCode.OK,
            false => StatusCode.BadRequest,
        };
    }
}

public static class ObjectIdExtensions
{
    public static ObjectId ToObjectId(this string objectId) => (ObjectId)objectId;
    public static string ToUrlEncoding(this ObjectId objectId) => Uri.EscapeDataString((string)objectId);
    public static ObjectId FromUrlEncoding(this string id) => Uri.UnescapeDataString(id).ToObjectId();
}