using System.Collections.Frozen;
using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace TicketShare.sdk;

public class HubChannelBuilder
{
    public string ChannelId { get; init; } = null!;
    public IDictionary<string, PrincipalChannelRecord> Users { get; init; } = null!;
    public IList<ChannelMessageRecord> Messages { get; init; } = null!;

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
        if (channel.ReadMessageIds.Any(x => x.MessageId == messageId)) return this;

        var newChannel = channel with
        {
            ReadMessageIds = channel.ReadMessageIds
                .Append(new ReadMessageRecord { MessageId = messageId, ReadDate = readDate })
                .ToImmutableArray(),
        };

        Users[principalId] = newChannel;
        return this;
    }

    public HubChannelRecord Build() => this.ConvertTo();
}


public static class HubChannelBuilderTool
{
    public static HubChannelBuilder ToBuilder(this HubChannelRecord subject) => new HubChannelBuilder
    {
        ChannelId = subject.ChannelId,
        Users = subject.Users.ToDictionary(),
        Messages = subject.Messages.ToList(),
    };

    public static HubChannelRecord ConvertTo(this HubChannelBuilder subject) => new HubChannelRecord
    {
        ChannelId = subject.ChannelId,
        Users = subject.Users.ToFrozenDictionary(),
        Messages = subject.Messages.ToImmutableArray(),
    };
}