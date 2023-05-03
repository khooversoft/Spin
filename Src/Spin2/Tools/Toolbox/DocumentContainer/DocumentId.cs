using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Toolbox.Application;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.DocumentContainer;

/// <summary>
/// Id is a path of {domain}/{resource}
/// </summary>
public sealed record DocumentId
{
    private string? _domain;
    private string? _path;
    private IReadOnlyList<string>? _vectors;

    public DocumentId(string id)
    {
        id.NotEmpty(name: id);

        Id = id.ToLower();
        IsDocumentIdValid(id).Assert(x => x.IsValid, x => x.Message ?? "< no error >");
    }

    public string Id { get; }

    [JsonIgnore]
    public string? Domain => _domain ??= Id.Split(':').Func(x => x.Length == 1 ? null : x[0]);

    [JsonIgnore]
    public string Path => _path ??= Id.Split(':').Func(x => x.Length == 1 ? x[0] : x[1]);

    [JsonIgnore]
    public IReadOnlyList<string> Vectors => _vectors ??= Path.Split('/');

    public override string ToString() => Id;

    public bool Equals(DocumentId? other) => other is not null && Id == other.Id;

    public override int GetHashCode() => HashCode.Combine(Id);

    public static implicit operator DocumentId(string documentId) => new DocumentId(documentId);

    public static implicit operator string(DocumentId documentId) => documentId.ToString();

    public static (bool IsValid, string? Message) IsDocumentIdValid(string documentId)
    {
        const string syntax = "[{domain}:]{path}[/{path}...]";

        if (documentId.IsEmpty()) return (false, "Id required");

        return Regex.Match(documentId, ToolboxConstants.DocumentIdRegexPattern, RegexOptions.IgnoreCase).Success switch
        {
            true => (true, string.Empty),
            false => (false, syntax),
        };
    }
}


public static class DocumentIdExtensions
{
    public static RunEnvironment GetRunEnvironment(this DocumentId documentId)
    {
        documentId.NotNull();
        return (RunEnvironment)Enum.Parse(typeof(RunEnvironment), documentId.Path.Split('/').First());
    }

    public static DocumentId ToDocumentId(this string subject) => (DocumentId)subject;
}
