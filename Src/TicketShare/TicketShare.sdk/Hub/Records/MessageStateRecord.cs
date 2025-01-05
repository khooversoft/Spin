using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public sealed record MessageStateRecord
{
    public string MessageId { get; init; } = null!;
    public DateTime ReadDate { get; init; }

    public bool Equals(MessageStateRecord? obj) => obj is MessageStateRecord subject &&
        MessageId == subject.MessageId &&
        ReadDate == subject.ReadDate;

    public override int GetHashCode() => HashCode.Combine(MessageId, ReadDate);

    public static IValidator<MessageStateRecord> Validator { get; } = new Validator<MessageStateRecord>()
        .RuleFor(x => x.MessageId).NotEmpty()
        .RuleFor(x => x.ReadDate).ValidDateTime()
        .Build();
}

public static class ReadMessageRecordTool
{
    public static Option Validate(this MessageStateRecord subject) => MessageStateRecord.Validator.Validate(subject).ToOptionStatus();
}
