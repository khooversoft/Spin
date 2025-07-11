using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph;

public class DeleteCommandBuilder
{
    public bool IfExist { get; set; }
    public string NodeKey { get; private set; } = null!;

    public DeleteCommandBuilder SetIfExist(bool ifExist = true) => this.Action(x => x.IfExist = ifExist);

    public DeleteCommandBuilder SetNodeKey(string nodeKey)
    {
        NodeKey = nodeKey.NotEmpty();
        return this;
    }

    public string Build()
    {
        string? ifExist = IfExist ? "ifexist" : null;

        var seq = new[]
        {
            "delete node",
            ifExist,
            $"key={NodeKey}",
            ";"
        };

        string cmd = seq.Where(x => x.IsNotEmpty()).Join(" ");
        return cmd;
    }
}
