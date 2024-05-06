using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public sealed record GraphDataLinkExpanded
{
    public GraphDataSource DataSource { get; init; } = null!;
    public GraphDataLink DataLink { get; init; } = null!;
    public DataETag DataETag { get; init; }
}


public static class GraphDataLinkExpandedExtensions
{
    public static GraphDataLinkExpanded ExpandGraphDataSource(this GraphDataSource subject, string nodeKey)
    {
        subject.NotNull();
        nodeKey.NotEmpty();

        var fileId = GraphTool.CreateFileId(nodeKey, subject.Name);

        var result = new GraphDataLinkExpanded
        {
            DataSource = subject,
            DataLink = new GraphDataLink
            {
                Name = subject.Name,
                TypeName = subject.TypeName,
                Schema = subject.Schema,
                FileId = fileId,
            },
            DataETag = Convert.FromBase64String(subject.Data64).ToDataETag(),
        };

        return result;
    }
}
