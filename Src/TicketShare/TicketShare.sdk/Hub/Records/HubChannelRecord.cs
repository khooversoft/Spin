using System.Collections.Frozen;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public enum ChannelRole
{
    Reader,
    Contributor,
    Owner,
}

public sealed record HubChannelRecord
{
    // "hub-channel:user1@domain.com/private" - private channel
    // "hub-channel:{owner@domain.com}/{ticketGroupId}" - channel for a ticket group
    public string ChannelId { get; init; } = null!;
    public string Name { get; init; } = null!;

    // PrincipalId
    public IReadOnlyDictionary<string, PrincipalChannelRecord> Users { get; init; } = FrozenDictionary<string, PrincipalChannelRecord>.Empty;
    public IReadOnlyList<ChannelMessageRecord> Messages { get; init; } = Array.Empty<ChannelMessageRecord>();

    public bool Equals(HubChannelRecord? obj)
    {
        var result = obj is HubChannelRecord subject &&
            ChannelId == subject.ChannelId &&
            Name == subject.Name &&
            Users.DeepEquals(subject.Users) &&
            Messages.OrderBy(x => x.MessageId).SequenceEqual(subject.Messages.OrderBy(x => x.MessageId));

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(ChannelId, Users, Messages);

    public static IValidator<HubChannelRecord> Validator { get; } = new Validator<HubChannelRecord>()
        .RuleFor(x => x.ChannelId).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.Users).NotNull()
        .RuleForEach(x => x.Users.Values).Validate(PrincipalChannelRecord.Validator)
        .RuleForEach(x => x.Messages).Validate(ChannelMessageRecord.Validator)
        .Build();
}

public static class HubChannelTool
{
    public static Option Validate(this HubChannelRecord subject) => HubChannelRecord.Validator.Validate(subject).ToOptionStatus();

    public static int GetUnreadMessageCount(this HubChannelRecord subject, string principalId)
    {
        subject.NotNull(nameof(subject));
        principalId.NotEmpty(nameof(principalId));

        int result = subject.Messages.Count - (subject.Users.TryGetValue(principalId, out var record) ? record.MessageStates.Count : 0);
        return result;
    }
}

