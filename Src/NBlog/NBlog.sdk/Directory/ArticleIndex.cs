using System.Diagnostics.CodeAnalysis;
using Toolbox.Tools;

namespace NBlog.sdk;

public record ArticleIndex
{
    public ArticleIndex() { }

    [SetsRequiredMembers]
    public ArticleIndex(string indexName, string articleId, string fileId)
    {
        IndexName = indexName.NotEmpty();
        ArticleId = articleId.NotEmpty();
        FileId = fileId.NotEmpty();
    }

    public required string IndexName { get; init; } = null!;
    public required string ArticleId { get; init; } = null!;
    public required string FileId { get; init; } = null!;
}
