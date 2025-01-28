using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions;

// Channel -> SecurityGroup -> Users
// Account -> Users
// TicketGroup -> SecurityGroup -> Users

public sealed record ChannelRecord
{
    // "hub-channel:user1@domain.com/private" - private channel
    // "hub-channel:{owner@domain.com}/{ticketGroupId}" - channel for a ticket group
    public string ChannelId { get; init; } = null!;

    public string SecurityGroupId { get; init; } = null!;
    public string Name { get; init; } = null!;
    public IReadOnlyList<ChannelMessage> Messages { get; init; } = Array.Empty<ChannelMessage>();

    public bool Equals(ChannelRecord? obj)
    {
        var result = obj is ChannelRecord subject &&
            ChannelId == subject.ChannelId &&
            SecurityGroupId == subject.SecurityGroupId &&
            Name == subject.Name &&
            Messages.OrderBy(x => x.MessageId).SequenceEqual(subject.Messages.OrderBy(x => x.MessageId));

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(ChannelId, Messages);

    public static IValidator<ChannelRecord> Validator { get; } = new Validator<ChannelRecord>()
        .RuleFor(x => x.ChannelId).NotEmpty()
        .RuleFor(x => x.SecurityGroupId).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleForEach(x => x.Messages).Validate(ChannelMessage.Validator)
        .Build();
}

public static class ChannelRecordTool
{
    public static Option Validate(this ChannelRecord subject) => ChannelRecord.Validator.Validate(subject).ToOptionStatus();

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
}
