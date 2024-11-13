using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public sealed record ReadMessageRecord
{
    public string MessageId { get; init; } = null!;
    public DateTime ReadDate { get; init; }

    public bool Equals(ReadMessageRecord? obj) => obj is ReadMessageRecord subject &&
        MessageId == subject.MessageId &&
        ReadDate == subject.ReadDate;

    public override int GetHashCode() => HashCode.Combine(MessageId, ReadDate);

    public static IValidator<ReadMessageRecord> Validator { get; } = new Validator<ReadMessageRecord>()
        .RuleFor(x => x.MessageId).NotEmpty()
        .RuleFor(x => x.ReadDate).ValidDateTime()
        .Build();
}

public static class ReadMessageRecordTool
{
    public static Option Validate(this ReadMessageRecord subject) => ReadMessageRecord.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this ReadMessageRecord subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}
