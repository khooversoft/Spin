using System.Diagnostics.CodeAnalysis;
using Toolbox.Tools;

namespace NBlog.sdk;

public record ArticleReference
{
    public ArticleReference() { }

    [SetsRequiredMembers]
    public ArticleReference(string articleId, string orderBy)
    {
        ArticleId = articleId.NotEmpty();
        OrderBy = int.Parse(orderBy);
    }

    public required string ArticleId { get; init; }
    public required int OrderBy { get; init; }
}
