using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph;

public readonly struct NodeCreateCommand : IGraphEntityCommand
{
    public NodeCreateCommand(string nodeKey, string? tags)
    {
        NodeKey = nodeKey.NotEmpty();
        Tags = tags;
        IsEntityNode = false;
    }

    public NodeCreateCommand(string nodeKey, string? tags, string entityDataBase64)
    {
        NodeKey = nodeKey.NotEmpty();
        Tags = tags;
        IsEntityNode = true;
        EntityDataBase64 = entityDataBase64.NotEmpty();
    }

    public string NodeKey { get; }
    public string? Tags { get; }
    public bool IsEntityNode { get; }
    public string? EntityDataBase64 { get; }

    public string GetAddCommand()
    {
        string?[] cmds = [
            $"upsert node key={NodeKey}",
            Tags,
            EntityDataBase64 != null ? $"entity {{ '{EntityDataBase64}' }}" : null,
            ];

        string cmd = cmds.Where(x => x.IsNotEmpty()).Join(", ") + ';';
        return cmd;
    }

    public string GetDeleteCommand() => $"delete (key={NodeKey});";
    public string GetSearchCommand() => $"select (key={NodeKey});";
}
