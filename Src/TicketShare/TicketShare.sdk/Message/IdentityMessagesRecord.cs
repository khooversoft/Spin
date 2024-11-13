//using System.Collections.Immutable;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace TicketShare.sdk;

//public sealed record class IdentityMessagesRecord
//{
//    public string PrincipalId { get; init; } = null!;   // Owner

//    public IReadOnlyList<MessageItemRecord> Messages { get; init; } = Array.Empty<MessageItemRecord>();

//    public bool Equals(IdentityMessagesRecord? obj) => obj is IdentityMessagesRecord subject &&
//        PrincipalId == subject.PrincipalId &&
//        Enumerable.SequenceEqual(Messages, subject.Messages);

//    public override int GetHashCode() => HashCode.Combine(PrincipalId, Messages);

//    public static IValidator<IdentityMessagesRecord> Validator { get; } = new Validator<IdentityMessagesRecord>()
//        .RuleFor(x => x.PrincipalId).NotEmpty()
//        .RuleForEach(x => x.Messages).Validate(MessageItemRecord.Validator)
//        .Build();
//}


//public static class IdentityMessageRecordTool
//{
//    public static Option Validate(this IdentityMessagesRecord subject) => IdentityMessagesRecord.Validator.Validate(subject).ToOptionStatus();

//    public static bool Validate(this IdentityMessagesRecord subject, out Option result)
//    {
//        result = subject.Validate();
//        return result.IsOk();
//    }

//    public static IReadOnlyList<MessageItemRecord> UnreadMessage(this IdentityMessagesRecord subject) =>
//        subject.Messages
//        .Where(x => x.ReadDate == null)
//        .ToImmutableArray();
//}