using Toolbox.Types;

namespace NBlog.sdk;

public record ArticleDetail
{
    public ArticleManifest Manifest { get; init; } = null!;
    public MarkdownDoc MarkdownDoc { get; init; } = null!;
    public string? ImageBase64 { get; init; }
}
