//using System.Collections.Frozen;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace TicketShare.sdk;

//public enum ChannelRole
//{
//    Reader = 0,
//    Contributor = 1,
//    Owner = 2,
//}

//public sealed record HubChannelRecord
//{
//    // "hub-channel:user1@domain.com/private" - private channel
//    // "hub-channel:{owner@domain.com}/{ticketGroupId}" - channel for a ticket group
//    public string ChannelId { get; init; } = null!;
//    public string Name { get; init; } = null!;

//    // PrincipalId
//    public IReadOnlyDictionary<string, PrincipalRoleRecord> Users { get; init; } = FrozenDictionary<string, PrincipalRoleRecord>.Empty;
//    public IReadOnlyList<ChannelMessageRecord> Messages { get; init; } = Array.Empty<ChannelMessageRecord>();

//    public bool Equals(HubChannelRecord? obj)
//    {
//        var result = obj is HubChannelRecord subject &&
//            ChannelId == subject.ChannelId &&
//            Name == subject.Name &&
//            Users.DeepEquals(subject.Users) &&
//            Messages.OrderBy(x => x.MessageId).SequenceEqual(subject.Messages.OrderBy(x => x.MessageId));

//        return result;
//    }

//    public override int GetHashCode() => HashCode.Combine(ChannelId, Users, Messages);

//    public static IValidator<HubChannelRecord> Validator { get; } = new Validator<HubChannelRecord>()
//        .RuleFor(x => x.ChannelId).NotEmpty()
//        .RuleFor(x => x.Name).NotEmpty()
//        .RuleFor(x => x.Users).NotNull()
//        .RuleForEach(x => x.Users.Values).Validate(PrincipalRoleRecord.Validator)
//        .RuleForEach(x => x.Messages).Validate(ChannelMessageRecord.Validator)
//        .Build();
//}

//public static class HubChannelTool
//{
//    public static Option Validate(this HubChannelRecord subject) => HubChannelRecord.Validator.Validate(subject).ToOptionStatus();

//    public static bool HasUnreadMessages(this HubChannelRecord subject, string principalId)
//    {
//        if (!subject.Users.TryGetValue(principalId, out PrincipalRoleRecord? principalChannelRecord)) return false;
//        return subject.Messages.Any(x => !principalChannelRecord.IsRead(x.MessageId));
//    }

//    public static bool HasAccess(this HubChannelRecord subject, string principalId, ChannelRole requiredAccess)
//    {
//        if (!subject.Users.TryGetValue(principalId, out PrincipalRoleRecord? principalChannelRecord)) return false;
//        return principalChannelRecord.HasAccess(requiredAccess);
//    }

//    public static Option HasAccess(this HubChannelRecord subject, string principalId, ChannelRole requiredAccess, ScopeContext context)
//    {
//        if (subject.HasAccess(principalId, requiredAccess)) return StatusCode.OK;

//        context.LogError("Principal does not have access to channelId={channelId}, principalId={principalId}", subject.ChannelId, principalId);
//        return StatusCode.Forbidden;
//    }
//}

