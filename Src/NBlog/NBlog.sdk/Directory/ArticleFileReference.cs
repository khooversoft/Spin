using System.Diagnostics.CodeAnalysis;
using Toolbox.Tools;

namespace NBlog.sdk;

public record ArticleFileReference
{
    public ArticleFileReference() { }

    [SetsRequiredMembers]
    public ArticleFileReference(string articleId, string createdDate, string fileId)
    {
        ArticleId = articleId.NotEmpty();
        FileId = fileId.NotEmpty();
        CreatedDate = DateTime.Parse(createdDate);
    }

    [SetsRequiredMembers]
    public ArticleFileReference(string articleId, DateTime createdDate, string fileId)
    {
        ArticleId = articleId.NotEmpty();
        FileId = fileId.NotEmpty();
        CreatedDate = createdDate;
    }

    public required string ArticleId { get; init; }
    public required DateTime CreatedDate { get; init; }
    public required string FileId { get; init; }
}
