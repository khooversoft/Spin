using Toolbox.Tools;

namespace Toolbox.Graph.Extensions;

public static class ChannelTool
{
    public const string NodeTag = "channel";
    public const string NodeKeyPrefix = "channel:";
    public const string EdgeType = "channel-principalGroup";
    public static string ToNodeKey(string channelId) => $"{NodeKeyPrefix}{channelId.NotEmpty().ToLower()}";
    public static string RemoveNodeKeyPrefix(string subject) => subject.NotEmpty().StartsWith(NodeKeyPrefix) switch
    {
        false => subject,
        true => subject[NodeKeyPrefix.Length..],
    };


}
