using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph;

public readonly struct NodeCreateCommand : IGraphEntityCommand
{
    public NodeCreateCommand(string nodeKey, string? tags = null, bool isEntityNode = false)
    {
        NodeKey = nodeKey.NotEmpty();
        Tags = tags;
        IsEntityNode = isEntityNode;
    }

    public string NodeKey { get; }
    public string? Tags { get; }
    public bool IsEntityNode { get; }

    public string GetAddCommand() => $"upsert node key={NodeKey}" + (Tags.IsNotEmpty() ? ", " + Tags : string.Empty) + ";";
    public string GetDeleteCommand() => $"delete (key={NodeKey});";
    public string GetSearchCommand() => $"select (key={NodeKey});";
}
