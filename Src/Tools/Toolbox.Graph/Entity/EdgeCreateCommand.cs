using Toolbox.Tools;

namespace Toolbox.Graph;

public readonly struct EdgeCreateCommand : IGraphEntityCommand
{
    public EdgeCreateCommand(string fromKey, string toKey) => (FromKey, ToKey) = (fromKey.NotEmpty(), toKey.NotEmpty());
    public string FromKey { get; }
    public string ToKey { get; }
    public string GetAddCommand() => $"add unique edge fromKey={FromKey}, toKey={ToKey}, edgeType={GraphConstants.UniqueIndexTag};";
    public string GetDeleteCommand() => $"delete [fromKey={FromKey}, toKey={ToKey}, edgeType={GraphConstants.UniqueIndexTag}];";
}
