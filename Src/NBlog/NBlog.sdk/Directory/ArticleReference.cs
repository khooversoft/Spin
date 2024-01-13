using System.Diagnostics.CodeAnalysis;
using Toolbox.Tools;

namespace NBlog.sdk;

public record ArticleReference
{
    public ArticleReference() { }

    [SetsRequiredMembers]
    public ArticleReference(string articleId, string index)
    {
        ArticleId = articleId.NotEmpty();
        Index = index;
    }

    public required string ArticleId { get; init; }
    public required string Index { get; init; }
}
