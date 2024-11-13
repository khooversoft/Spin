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
    public string OwnerPrincipalId { get; init; } = null!;

    // PrincipalId
    public IReadOnlyDictionary<string, PrincipalChannelRecord> Users { get; init; } = FrozenDictionary<string, PrincipalChannelRecord>.Empty;
    public IReadOnlyList<ChannelMessageRecord> Messages { get; init; } = Array.Empty<ChannelMessageRecord>();

    public IReadOnlyList<MessageState> GetMessages(string principalId)
    {
        if (!Users.TryGetValue(principalId, out PrincipalChannelRecord? record)) return Array.Empty<MessageState>();

        var list = Messages
            .Select(x => new MessageState(x, record.IsRead(x.MessageId)))
            .ToImmutableArray();

        return list;
    }

    public HubChannelRecord WithMessageRead(string principalId, IEnumerable<string> messageIds, DateTime readDate)
    {
        principalId.NotEmpty();
        messageIds.NotNull().ForEach(x => x.NotEmpty());

        if (!Users.TryGetValue(principalId, out PrincipalChannelRecord? channel)) return this;

        var messages = Messages
            .Join(messageIds, x => x.MessageId, x => x, (x, _) => x.MessageId, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var newMessageIds = messageIds
            .Except(channel.ReadMessageIds.Select(x => x.MessageId))
            .ToArray();

        var newChannel = channel with
        {
            ReadMessageIds = channel.ReadMessageIds
                .Select(x => messages.Contains(x.MessageId) ? x with { ReadDate = readDate } : x)
                .Concat(newMessageIds.Select(x => new ReadMessageRecord { MessageId = x, ReadDate = readDate }))
                .ToImmutableArray(),
        };

        var newUsers = Users.ToDictionary();
        newUsers[principalId] = newChannel;

        return this with { Users = newUsers.ToFrozenDictionary() };
    }

    public HubChannelRecord WithMessageAdd(ChannelMessageRecord message)
    {
        message.Validate().ThrowOnError();

        return this with
        {
            Messages = Messages.Append(message).ToImmutableArray()
        };
    }

    public bool Equals(HubChannelRecord? obj) => obj is HubChannelRecord subject &&
        ChannelId == subject.ChannelId &&
        OwnerPrincipalId == subject.OwnerPrincipalId &&
        Users.DeepEquals(subject.Users) &&
        Messages.OrderBy(x => x.MessageId).SequenceEqual(subject.Messages.OrderBy(x => x.MessageId));

    public override int GetHashCode() => HashCode.Combine(ChannelId, OwnerPrincipalId, Users, Messages);

    public static IValidator<HubChannelRecord> Validator { get; } = new Validator<HubChannelRecord>()
        .RuleFor(x => x.ChannelId).NotEmpty()
        .RuleFor(x => x.OwnerPrincipalId).NotEmpty()
        .RuleFor(x => x.Users).NotNull()
        .RuleForEach(x => x.Users.Values).Validate(PrincipalChannelRecord.Validator)
        .RuleForEach(x => x.Messages).Validate(ChannelMessageRecord.Validator)
        .Build();
}

public readonly struct MessageState
{
    public MessageState(ChannelMessageRecord message, DateTime? readDate)
    {
        Message = message.NotNull();
        ReadDate = readDate;
    }

    public ChannelMessageRecord Message { get; }
    public DateTime? ReadDate { get; init; }
}

public static class HubChannelTool
{
    public static Option Validate(this HubChannelRecord subject) => HubChannelRecord.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this HubChannelRecord subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}

