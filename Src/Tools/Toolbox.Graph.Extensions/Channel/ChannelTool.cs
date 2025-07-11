using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions;

public static class ChannelTool
{
    public const string NodeTag = "channel";
    public const string NodeKeyPrefix = "channel:";
    public const string EdgeType = "channel-securityGroup";
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

    public static Option<string> CreateQuery(this ChannelRecord channelRecord, bool useSet, ScopeContext context)
    {
        if (channelRecord.Validate().IsError(out var r)) return r.LogStatus(context, nameof(ChannelRecord)).ToOptionStatus<string>();

        string nodeKey = ChannelTool.ToNodeKey(channelRecord.ChannelId);

        var cmd = new NodeCommandBuilder()
            .UseSet(useSet)
            .SetNodeKey(nodeKey)
            .AddTag(ChannelTool.NodeTag)
            .AddData("entity", channelRecord)
            .AddReference(ChannelTool.EdgeType, SecurityGroupTool.ToNodeKey(channelRecord.SecurityGroupId))
            .Build();

        return cmd;
    }

    public static ChannelRecord CreateRecord(string channelId, string securityGroupId, string name) => new ChannelRecord
    {
        ChannelId = channelId.NotEmpty(),
        SecurityGroupId = securityGroupId.NotEmpty(),
        Name = name.NotEmpty(),
    };

    public static CommandBatchBuilder AddChannel(this CommandBatchBuilder builder, ChannelRecord subject, bool useSet)
    {
        builder.Add((context) => CreateQuery(subject, useSet, context));
        return builder;
    }
}
