using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public record Node
{
    public Node(string nodeKey, DataETag? payload = null)
    {
        NodeKey = nodeKey.NotEmpty();
        Payload = payload;
    }

    public string NodeKey { get; } = null!;
    public DataETag? Payload { get; init; }

    public static IValidator<Node> Validator { get; } = new Validator<Node>()
        .RuleFor(x => x.NodeKey).NotEmpty()
        .Build();
}

public static class NodeTool
{
    public static Option Validate(this Node node) => Node.Validator.Validate(node).ToOptionStatus();

    public static string CreateKey(string nodeKey, string? type)
    {
        nodeKey.NotEmpty();

        var result = type switch
        {
            null => nodeKey,
            string t => nodeKey.IndexOf(':') switch
            {
                -1 => $"{t}:{nodeKey}",
                _ => ParseKey(nodeKey) switch
                {
                    { NodeType: var existingType } v when existingType.EqualsIgnoreCase(t) => nodeKey,
                    var v => $"{t}:{v.NodeKey}",
                }
            }
        };

        return result.ToLowerInvariant();
    }

    public static (string NodeKey, string? NodeType) ParseKey(string key)
    {
        key.NotEmpty();

        var result = key.Split(':') switch
        {
            { Length: 1 } v => (NodeKey: v[0], NodeType: (string?)null),
            { Length: 2 } v => (NodeKey: v[1], NodeType: v[0]),
            _ => throw new FormatException($"Invalid key format: {key}")
        };

        return result;
    }
}
