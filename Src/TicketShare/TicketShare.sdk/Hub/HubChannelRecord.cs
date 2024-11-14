using System.Collections.Frozen;
using System.Collections.Immutable;
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

    // PrincipalId
    public IReadOnlyDictionary<string, PrincipalChannelRecord> Users { get; init; } = FrozenDictionary<string, PrincipalChannelRecord>.Empty;
    public IReadOnlyList<ChannelMessageRecord> Messages { get; init; } = Array.Empty<ChannelMessageRecord>();

    public bool Equals(HubChannelRecord? obj) => obj is HubChannelRecord subject &&
        ChannelId == subject.ChannelId &&
        Users.DeepEquals(subject.Users) &&
        Messages.OrderBy(x => x.MessageId).SequenceEqual(subject.Messages.OrderBy(x => x.MessageId));

    public override int GetHashCode() => HashCode.Combine(ChannelId, Users, Messages);

    public static IValidator<HubChannelRecord> Validator { get; } = new Validator<HubChannelRecord>()
        .RuleFor(x => x.ChannelId).NotEmpty()
        .RuleFor(x => x.Users).NotNull().Must(x => x.Values.Count(x => x.Role == ChannelRole.Owner) > 0, _ => "Must have a least 1 owner")
        .RuleForEach(x => x.Users.Values).Validate(PrincipalChannelRecord.Validator)
        .RuleForEach(x => x.Messages).Validate(ChannelMessageRecord.Validator)
        .Build();
}

public static class HubChannelTool
{
    public static Option Validate(this HubChannelRecord subject) => HubChannelRecord.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this HubChannelRecord subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }

    public static IReadOnlyList<MessageState> GetMessages(this HubChannelRecord subject, string? principalId = null)
    {
        PrincipalChannelRecord? record = null;

        if (principalId.IsNotEmpty()) subject.Users.TryGetValue(principalId, out record);

        var list = subject.Messages
            .Select(x => new MessageState(x, record?.IsRead(x.MessageId)))
            .ToImmutableArray();

        return list;
    }
}

