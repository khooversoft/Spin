using System.Collections.Frozen;
using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace TicketShare.sdk;

public class HubChannelBuilder
{
    public HubChannelBuilder(string channelId, IReadOnlyDictionary<string, PrincipalChannelRecord> users, IReadOnlyList<ChannelMessageRecord> messages)
    {
        ChannelId = channelId;
        Users = users.ToDictionary();
        Messages = messages.ToList();
    }

    public string ChannelId { get; } = null!;
    public IDictionary<string, PrincipalChannelRecord> Users { get; } = null!;
    public IList<ChannelMessageRecord> Messages { get; } = null!;

    public HubChannelBuilder AddMessage(ChannelMessageRecord message) => this.Action(x => x.Messages.Add(message));
    public HubChannelBuilder AddUser(PrincipalChannelRecord user) => this.Action(x => x.Users.Add(user.PrincipalId, user));

    public HubChannelBuilder RemoveUser(string principalId)
    {
        if (!Users.TryGetValue(principalId, out PrincipalChannelRecord? record)) return this;
        if (record.Role == ChannelRole.Owner && Users.Values.Count(x => x.Role == ChannelRole.Owner) == 1) return this;

        Users.Remove(principalId);
        return this;
    }

    public HubChannelBuilder MarkRead(string principalId, string messageId, DateTime readDate)
    {
        principalId.NotEmpty();

        if (!Users.TryGetValue(principalId, out PrincipalChannelRecord? channel)) return this;
        if (!Messages.Any(x => x.MessageId == messageId)) return this;

        var newChannel = channel with
        {
            MessageStates = channel.MessageStates
                .ToDictionary()
                .Action(x => x.TryAdd(messageId, new MessageStateRecord { MessageId = messageId, ReadDate = readDate }))
                .ToFrozenDictionary()
        };

        Users[principalId] = newChannel;
        return this;
    }

    public HubChannelRecord Build() => new HubChannelRecord
    {
        ChannelId = ChannelId,
        Users = Users.ToFrozenDictionary(),
        Messages = Messages.ToImmutableArray(),
    };
}


public static class HubChannelBuilderTool
{
    public static HubChannelBuilder ToBuilder(this HubChannelRecord subject) => new HubChannelBuilder(subject.ChannelId, subject.Users, subject.Messages);
}