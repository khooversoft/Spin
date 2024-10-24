using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class NodeCommandBuilder
{
    public bool Set { get; set; }
    public string NodeKey { get; private set; } = null!;
    public IDictionary<string, string?> Tags { get; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
    public IDictionary<string, string> Data { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> Indexes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public NodeCommandBuilder UseAdd() => this.Action(x => x.Set = false);
    public NodeCommandBuilder UseSet() => this.Action(x => x.Set = true);

    public NodeCommandBuilder SetNodeKey(string nodeKey)
    {
        NodeKey = nodeKey.NotEmpty();
        return this;
    }

    public NodeCommandBuilder AddTag(string tag)
    {
        tag.NotEmpty();

        (string key, string? value) = tag.Split('=') switch
        {
            { Length: 1 } => (tag, null),
            { Length: 2 } v => (v[0], v[1]),
            _ => throw new ArgumentException("Invalid format"),
        };

        Tags[key] = value;
        return this;
    }

    public NodeCommandBuilder AddTag(string tag, string? value)
    {
        tag.NotEmpty();
        value.NotEmpty();

        Tags[tag] = value;
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
