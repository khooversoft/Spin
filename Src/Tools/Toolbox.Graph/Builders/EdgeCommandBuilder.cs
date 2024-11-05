using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class EdgeCommandBuilder
{
    private TagCollection _tagCollection = new TagCollection();

    public EdgeCommandBuilder() { }
    public EdgeCommandBuilder(string fromKey, string toKey, string edgeType)
    {
        FromKey = fromKey.NotEmpty();
        ToKey = toKey.NotEmpty();
        EdgeType = edgeType.NotEmpty();
    }

    public bool Set { get; set; }
    public string FromKey { get; private set; } = null!;
    public string ToKey { get; private set; } = null!;
    public string EdgeType { get; private set; } = null!;
    public IDictionary<string, string?> Tags => _tagCollection.Tags;

    public EdgeCommandBuilder UseAdd() => this.Action(x => x.Set = false);
    public EdgeCommandBuilder UseSet(bool useSet = true) => this.Action(x => x.Set = useSet);

    public EdgeCommandBuilder SetFromKey(string fromKey) => this.Action(_ => FromKey = fromKey.NotEmpty());
    public EdgeCommandBuilder SetToKey(string toKey) => this.Action(_ => ToKey = toKey.NotEmpty());
    public EdgeCommandBuilder SetEdgeType(string edgeType) => this.Action(_ => EdgeType = edgeType.NotEmpty());
    public EdgeCommandBuilder AddTag(string tag) => this.Action(_ => _tagCollection.AddTag(tag));
    public EdgeCommandBuilder AddTag(string tag, string? value) => this.Action(_ => _tagCollection.AddTag(tag, value));

    public string Build()
    {
        string opr = Set ? "set" : "add";

        string tags = Tags.ToTagsString();
        string? setCmd = tags.IsNotEmpty() ? "set" : null;

        string keys = new[] { $"from={FromKey}", $"to={ToKey}", $"type={EdgeType}" }.Join(", ");

        var seq = new[]
        {
            opr,
            "edge",
            keys,
            setCmd,
            tags,
            ";"
        };

        string cmd = seq.Where(x => x.IsNotEmpty()).Join(" ");
        return cmd;
    }
}
