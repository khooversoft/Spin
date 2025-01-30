using Toolbox.Tools;

namespace Toolbox.Graph.Extensions;

public static class ChannelTool
{
    public const string NodeTag = "channel";
    public const string NodeKeyPrefix = "channel:";
    public const string EdgeType = "channel-principalGroup";
    public const string ViewNodeTag = "/channel-view";

    public static string ToNodeKey(string channelId) => $"{NodeKeyPrefix}{channelId.NotEmpty().ToLower()}";

    public static string CleanNodeKey(string subject) => subject.NotEmpty().StartsWith(NodeKeyPrefix) switch
    {
        false => subject,
        true => subject[NodeKeyPrefix.Length..],
    };

    public static string ToViewNodeKey(string channelId) => ToNodeKey(channelId) + ViewNodeTag;

    public static string CleanViewNodeKey(string subject)
    {
        subject.NotNull();

        string c1 = CleanNodeKey(subject);

        var c2 = subject.EndsWith(ViewNodeTag) switch
        {
            false => c1,
            true => subject[..^(ViewNodeTag.Length)],
        };

        return c2;
    }
}
