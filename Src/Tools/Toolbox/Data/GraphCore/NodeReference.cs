using Toolbox.Tools;

namespace Toolbox.Data;

public class NodeReference
{
    public NodeReference(string nodeKey) => NodeKey = nodeKey.NotEmpty();

    public string NodeKey { get; }
}
