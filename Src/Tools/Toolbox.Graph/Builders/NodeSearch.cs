using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class NodeSearch : ISelectSearch
{
    private TagCollection _tagCollection = new TagCollection();

    public string? NodeKey { get; private set; }
    public IDictionary<string, string?> Tags => _tagCollection.Tags;

    public NodeSearch SetNodeKey(string nodeKey) => this.Action(_ => NodeKey = nodeKey.NotEmpty());
    public NodeSearch AddTag(string tag) => this.Action(_ => _tagCollection.AddTag(tag));
    public NodeSearch AddTag(string tag, string? value) => this.Action(_ => _tagCollection.AddTag(tag, value));

    public string Build()
    {
        string? nodeKey = NodeKey.IsNotEmpty() ? $"key={NodeKey}" : null;
        string tags = Tags.ToTagsString();

        string search = new[] { nodeKey, tags }.Where(x => x.IsNotEmpty()).Join(", ");
        string cmd = "(" + search + ")";
        return cmd;
    }
}
