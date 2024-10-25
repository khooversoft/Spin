using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class NodeCommandBuilder
{
    private TagCollection _tagCollection = new TagCollection();

    public bool Set { get; set; }
    public string NodeKey { get; private set; } = null!;
    public IDictionary<string, string?> Tags => _tagCollection.Tags;
    public IDictionary<string, string> Data { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> Indexes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public NodeCommandBuilder UseAdd() => this.Action(x => x.Set = false);
    public NodeCommandBuilder UseSet() => this.Action(x => x.Set = true);
    public NodeCommandBuilder AddTag(string tag) => this.Action(_ => _tagCollection.AddTag(tag));
    public NodeCommandBuilder AddTag(string tag, string? value) => this.Action(_ => _tagCollection.AddTag(tag, value));

    public NodeCommandBuilder SetNodeKey(string nodeKey)
    {
        NodeKey = nodeKey.NotEmpty();
        return this;
    }

    public NodeCommandBuilder AddData(string name, string value)
    {
        name.NotEmpty();
        value.NotEmpty();

        Data[name] = value;
        return this;
    }

    public NodeCommandBuilder AddIndex(string name)
    {
        name.NotEmpty();

        Indexes.Add(name);
        return this;
    }

    public NodeCommandBuilder AddData<T>(string name, T value)
    {
        name.NotEmpty();
        value.NotNull();

        Data[name] = value.ToJson().ToBase64();
        return this;
    }

    public string Build()
    {
        string opr = Set ? "set" : "add";

        string tags = Tags.ToTagsString();
        string data = Data.Select(x => $"{x.Key} {{ '{x.Value}' }}").Join(", ");
        string tagsData = new[] { tags, data }.Where(x => x.IsNotEmpty()).Join(", ");

        string indexes = Indexes.Join(", ");

        string? setCmd = tags.IsNotEmpty() || data.IsNotEmpty() ? "set" : null;
        string? indexesCmd = indexes.IsNotEmpty() ? "index" : null;

        var seq = new[]
        {
            opr,
            "node",
            $"key={NodeKey}",
            setCmd,
            tagsData,
            indexesCmd,
            indexes,
            ";"
        };

        string cmd = seq.Where(x => x.IsNotEmpty()).Join(" ");
        return cmd;
    }
}
