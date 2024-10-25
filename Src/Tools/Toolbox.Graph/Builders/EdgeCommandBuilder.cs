using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class EdgeCommandBuilder
{
    private TagCollection _tagCollection = new TagCollection();

    public bool Set { get; set; }
    public string FromKey { get; private set; } = null!;
    public string ToKey { get; private set; } = null!;
    public string EdgeType { get; private set; } = null!;
    public IDictionary<string, string?> Tags => _tagCollection.Tags;

    public EdgeCommandBuilder UseAdd() => this.Action(x => x.Set = false);
    public EdgeCommandBuilder UseSet() => this.Action(x => x.Set = true);

    public EdgeCommandBuilder SetFromKey(string fromKey) => this.Action(_ => FromKey = fromKey.NotEmpty());
    public EdgeCommandBuilder SetToKey(string toKey) => this.Action(_ => ToKey = toKey.NotEmpty());
    public EdgeCommandBuilder SetEdgeType(string edgeType) => this.Action(_ => EdgeType = edgeType.NotEmpty());
    public EdgeCommandBuilder SetTag(string tag) => this.Action(_ => _tagCollection.AddTag(tag));
    public EdgeCommandBuilder SetTag(string tag, string? value) => this.Action(_ => _tagCollection.AddTag(tag, value));

    public string Build()
    {
        string opr = Set ? "set" : "add";

        string tags = Tags.ToTagsString();
        string? setCmd = tags.IsNotEmpty() ? "set" : null;

        var seq = new[]
        {
            opr,
            "edge",
            $"from={FromKey}",
            $"to={ToKey}",
            $"type={EdgeType}",
            setCmd,
            tags,
            ";"
        };

        string cmd = seq.Where(x => x.IsNotEmpty()).Join(" ");
        return cmd;
    }
}
