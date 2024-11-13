using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class NodeCommandBuilder
{
    private TagCollection _tagCollection = new TagCollection();

    public NodeCommandBuilder() { }
    public NodeCommandBuilder(string nodeKey) => NodeKey = nodeKey.NotEmpty();
    public NodeCommandBuilder(string nodeKey, bool set) => (Set, NodeKey) = (set, nodeKey.NotEmpty());

    public bool Set { get; set; }
    public string NodeKey { get; private set; } = null!;
    public IDictionary<string, string?> Tags => _tagCollection.Tags;
    public IDictionary<string, string> Data { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> Indexes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public IDictionary<string, string?> ForeignKeys { get; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    public NodeCommandBuilder SetNodeKey(string nodeKey) => this.Action(x => x.NodeKey = nodeKey.NotEmpty());

    public NodeCommandBuilder UseAdd() => this.Action(x => x.Set = false);
    public NodeCommandBuilder UseSet(bool useSet = true) => this.Action(x => x.Set = useSet);
    public NodeCommandBuilder AddTag(string tag) => this.Action(_ => _tagCollection.AddTag(tag));
    public NodeCommandBuilder AddTag(string tag, string? value = null, bool isReference = false) => this.Action(_ =>
    {
        _tagCollection.AddTag(tag, value);
        if (isReference) ForeignKeys.Add(tag, null);
    });

    public NodeCommandBuilder AddData(string name, string value) => this.Action(x => x.Data[name.NotEmpty()] = value.NotEmpty());
    public NodeCommandBuilder AddIndex(string name) => this.Action(x => x.Indexes.Add(name.NotEmpty()));
    public NodeCommandBuilder AddForeignKey(string name, string? pattern = null) => this.Action(x => x.ForeignKeys.Add(name, pattern));

    public NodeCommandBuilder AddData<T>(string name, T value)
    {
        name.NotEmpty();
        value.NotNull();

        Data[name] = value.ToJson().ToBase64();
        return this;
    }

    public NodeCommandBuilder AddReference(string edgeType, string reference)
    {
        edgeType.NotEmpty();
        reference.NotEmpty();

        if (!ForeignKeys.ContainsKey(edgeType)) ForeignKeys.Add(edgeType, null);
        Tags.Add(edgeType, reference);

        return this;
    }

    public NodeCommandBuilder AddReferences(string edgeType, IEnumerable<string> references)
    {
        edgeType.NotEmpty();
        references.NotNull();
        if (!references.Any()) return this;

        if (!ForeignKeys.ContainsKey(edgeType)) ForeignKeys.Add(edgeType, $"{edgeType}-*");

        foreach (var value in references)
        {
            string hashEdgeType = GraphTool.ToHashTag(edgeType, value);
            if (Tags.ContainsKey(hashEdgeType)) continue;
            Tags.Add(hashEdgeType, value);
        }

        return this;
    }

    public string Build()
    {
        string opr = Set ? "set" : "add";

        string tags = Tags.ToTagsString();
        string data = Data.Select(x => $"{x.Key} {{ '{x.Value}' }}").Join(", ");
        string tagsData = new[] { tags, data }.Where(x => x.IsNotEmpty()).Join(", ");

        string indexes = Indexes.Join(", ");
        string foreignKeys = ForeignKeys.ToTagsString();

        string? setCmd = tags.IsNotEmpty() || data.IsNotEmpty() ? "set" : null;
        string? indexesCmd = indexes.IsNotEmpty() ? "index" : null;
        string? foreignKeyCmd = foreignKeys.IsNotEmpty() ? "foreignkey" : null;

        var seq = new[]
        {
            opr,
            "node",
            $"key={NodeKey}",
            setCmd,
            tagsData,
            indexesCmd,
            indexes,
            foreignKeyCmd,
            foreignKeys,
            ";"
        };

        string cmd = seq.Where(x => x.IsNotEmpty()).Join(" ");
        return cmd;
    }
}
