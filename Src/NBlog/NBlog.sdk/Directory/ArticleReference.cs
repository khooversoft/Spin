using System.Diagnostics.CodeAnalysis;
using Toolbox.Tools;

namespace NBlog.sdk;

public record ArticleReference
{
    public ArticleReference() { }

    [SetsRequiredMembers]
    public ArticleReference(string articleId, string createdDate)
    {
        ArticleId = articleId.NotEmpty();
        CreatedDate = DateTime.Parse(createdDate);
    }

    [SetsRequiredMembers]
    public ArticleReference(string articleId, DateTime createdDate)
    {
        ArticleId = articleId.NotEmpty();
        CreatedDate = createdDate;
    }

    public required string ArticleId { get; init; }
    public required DateTime CreatedDate { get; init; }
}
