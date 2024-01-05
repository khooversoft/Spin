using System.Text;
using ColorCode;
using Markdig;
using Markdown.ColorCode;
using Toolbox.Data;

namespace NBlog.sdk;

public class MarkdownDoc
{
    private string? _html;
    private static MarkdownPipeline? _pipeline;
    private readonly static string _csharpLanguageId = Languages.CSharp.Id;

    public MarkdownDoc(byte[] data) => MdSource = Encoding.UTF8.GetString(DataTool.RemoveBOM(data));

    public string MdSource { get; init; }

    public string ToHtml()
    {
        return _html ??= build();

        string build()
        {
            _pipeline ??= new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseBootstrap()
                .UseColorCode(defaultLanguageId: _csharpLanguageId)
                .UsePreciseSourceLocation()
                .UseYamlFrontMatter()
                .UseEmojiAndSmiley()
                .Build();

            return Markdig.Markdown.ToHtml(MdSource, _pipeline);
        }
    }
}
