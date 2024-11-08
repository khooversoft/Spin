using System.Collections.Frozen;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk.Hub;

//public sealed record IdentityHubRecord
//{
//    public string PrincipalId { get; init; } = null!;   // Owner

//    public IReadOnlyDictionary<string, HubChannel> Hubs { get; init; } = FrozenDictionary<string, HubChannel>.Empty;

//    public bool Equals(IdentityHubRecord? obj) => obj is IdentityHubRecord subject &&
//        PrincipalId == subject.PrincipalId &&
//        Hubs.DeepEquals(subject.Hubs);

//    public override int GetHashCode() => HashCode.Combine(PrincipalId, Hubs);

//    public static IValidator<IdentityHubRecord> Validator { get; } = new Validator<IdentityHubRecord>()
//        .RuleFor(x => x.PrincipalId).NotEmpty()
//        .RuleForEach(x => x.Hubs.Values).Validate(HubChannel.Validator)
//        .Build();
//}


public enum ChannelRole
{
    None,
    Reader,
    Contributor,
    Owner,
}

public sealed record HubChannel
{
    // "hub-channel:domain.com/user1@domain.com/private" - private channel
    // "hub-channel:domain.com/{ticketGroupId}" - channel for a ticket group
    public string ChannelId { get; init; } = null!;

    // PrincipalId
    public IReadOnlyDictionary<string, PrincipalChannel> Users { get; init; } = FrozenDictionary<string, PrincipalChannel>.Empty;
    public IReadOnlyList<ChannelMessage> Messages { get; init; } = Array.Empty<ChannelMessage>();

    public static IValidator<HubChannel> Validator { get; } = new Validator<HubChannel>()
        .RuleFor(x => x.ChannelId).NotEmpty()
        .RuleForEach(x => x.Users.Values).Validate(PrincipalChannel.Validator)
        .RuleForEach(x => x.Messages).Validate(ChannelMessage.Validator)
        .Build();
}

public sealed record PrincipalChannel
{
    public ChannelRole Role { get; init; } = ChannelRole.None;
    public string PrincipalId { get; init; } = null!;
    public IReadOnlyList<string> ReadMessageIds { get; init; } = Array.Empty<string>();

    public bool IsRead(string messageId) => ReadMessageIds.Contains(messageId, StringComparer.OrdinalIgnoreCase);

    public static IValidator<PrincipalChannel> Validator { get; } = new Validator<PrincipalChannel>()
        .RuleFor(x => x.Role).ValidEnum()
        .RuleFor(x => x.PrincipalId).NotEmpty()
        .RuleForEach(x => x.ReadMessageIds).NotEmpty()
        .Build();
}

public static class PrincipalChannelTool
{
    public static Option Validate(this PrincipalChannel subject) => PrincipalChannel.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this PrincipalChannel subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}


public sealed record ChannelMessage
{
    public string MessageId { get; init; } = Guid.NewGuid().ToString();
    public string FromPrincipalId { get; init; } = null!;
    public string ToChannelId { get; init; } = null!;
    public string Message { get; init; } = null!;
    public string? ProposalId { get; init; }
    public DateTime? ReadDate { get; init; }

    public static IValidator<ChannelMessage> Validator { get; } = new Validator<ChannelMessage>()
        .RuleFor(x => x.MessageId).NotEmpty()
        .RuleFor(x => x.FromPrincipalId).NotEmpty()
        .RuleFor(x => x.ToChannelId).NotEmpty()
        .RuleFor(x => x.Message).NotEmpty()
        .Build();
}

public static class MessageRecordTool
{
    public static Option Validate(this ChannelMessage subject) => ChannelMessage.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this ChannelMessage subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}