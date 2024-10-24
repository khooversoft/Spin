using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.Builders;

public class SelectCommandBuilder
{
    public string? NodeKey { get; private set; }
    public IDictionary<string, string?> Tags { get; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> DataNames { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public SelectCommandBuilder SetNodeKey(string nodeKey)
    {
        NodeKey = nodeKey.NotEmpty();
        return this;
    }

    public SelectCommandBuilder AddTag(string tag)
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

    public SelectCommandBuilder AddTag(string tag, string? value)
    {
        tag.NotEmpty();
        value.NotEmpty();

        Tags[tag] = value;
        return this;
    }

    public SelectCommandBuilder AddDataName(string name)
    {
        name.NotEmpty();

        DataNames.Add(name);
        return this;
    }

    public string Build()
    {
        string? nodeKey = NodeKey.IsNotEmpty() ? $"key={NodeKey}" : null;

        string tags = Tags.ToTagsString();
        string dataNames = DataNames.Join(", ");

        string search = new[] { nodeKey, tags }.Join(", ");
        string? returnOpr = dataNames.IsNotEmpty() ? "return" : null;

        var seq = new[]
        {
            "select",
            "(",
            search,
            ")",
            returnOpr,
            ";"
        };

        string cmd = seq.Where(x => x.IsNotEmpty()).Join(" ");
        return cmd;
    }
}

public interface ISelectSearch
{
    string Build();
}

public class NodeSearch : ISelectSearch
{
    public string? NodeKey { get; private set; }
    public IDictionary<string, string?> Tags { get; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    public string Build()
    {
        string? nodeKey = NodeKey.IsNotEmpty() ? $"key={NodeKey}" : null;
        string tags = Tags.ToTagsString();

        string search = new[] { nodeKey, tags }.Join(", ");

        var seq = new[]
        {
            "(",
            search,
            ")",
        };

        string cmd = seq.Where(x => x.IsNotEmpty()).Join(" ");
        return cmd;
    }
}

public class EdgeSearch : ISelectSearch
{
    public string? FromKey { get; private set; }
    public string? ToKey { get; private set; }
    public string? EdgeType { get; private set; }
    public IDictionary<string, string?> Tags { get; } = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    public string Build()
    {
        string? fromKey = FromKey.IsNotEmpty() ? $"key={FromKey}" : null;
        string? toKey = ToKey.IsNotEmpty() ? $"key={ToKey}" : null;
        string? edgeType = EdgeType.IsNotEmpty() ? $"type={EdgeType}" : null;
        string tags = Tags.ToTagsString();

        string search = new[] { fromKey, toKey, edgeType, tags }.Join(", ");

        var seq = new[]
        {
            "[",
            search,
            "]",
        };

        string cmd = seq.Where(x => x.IsNotEmpty()).Join(" ");
        return cmd;
    }
}