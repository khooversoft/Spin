using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class EdgeSearch : ISelectSearch
{
    private TagCollection _tagCollection = new TagCollection();

    public string? FromKey { get; private set; }
    public string? ToKey { get; private set; }
    public string? EdgeType { get; private set; }
    public EdgeSearch SetFromKey(string fromKey) => this.Action(_ => FromKey = fromKey.NotEmpty());
    public EdgeSearch SetToKey(string toKey) => this.Action(_ => ToKey = toKey.NotEmpty());
    public EdgeSearch SetEdgeType(string edgeType) => this.Action(_ => EdgeType = edgeType.NotEmpty());
    public IDictionary<string, string?> Tags => _tagCollection.Tags;
    public EdgeSearch AddTag(string tag) => this.Action(_ => _tagCollection.AddTag(tag));
    public EdgeSearch AddTag(string tag, string? value) => this.Action(_ => _tagCollection.AddTag(tag, value));

    public string Build()
    {
        string? fromKey = FromKey.IsNotEmpty() ? $"from={FromKey}" : null;
        string? toKey = ToKey.IsNotEmpty() ? $"to={ToKey}" : null;
        string? edgeType = EdgeType.IsNotEmpty() ? $"type={EdgeType}" : null;
        string tags = Tags.ToTagsString();

        string search = (fromKey.IsNotEmpty() || toKey.IsNotEmpty() || edgeType.IsNotEmpty() || tags.IsNotEmpty()) switch
        {
            true => new[] { fromKey, toKey, edgeType, tags }.Where(x => x.IsNotEmpty()).Join(", "),
            false => "*",
        };

        string cmd = "[" + search + "]";
        return cmd;
    }
}
