using System.Text;
using Markdig;
using Toolbox.Extensions;

namespace NBlog.sdk;

public class MarkdownDoc
{
    private string? _html;
    public MarkdownDoc(byte[] data) => MdSource = Encoding.UTF8.GetString(data.RemoveBOM());

    public string MdSource { get; init; }

    public string ToHtml()
    {
        return _html ??= build();

        string build()
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseBootstrap()
                .Build();

            return Markdown.ToHtml(MdSource, pipeline);
        }
    }
}
