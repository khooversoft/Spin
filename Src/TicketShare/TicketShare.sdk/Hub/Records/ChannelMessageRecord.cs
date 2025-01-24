using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public sealed record ChannelMessageRecord
{
    public string ChannelId { get; init; } = null!;
    public string MessageId { get; init; } = SequenceTool.GenerateId();
    public DateTime Date { get; init; } = DateTime.UtcNow;
    public string FromPrincipalId { get; init; } = null!;
    public string Message { get; init; } = null!;

    public bool Equals(ChannelMessageRecord? obj)
    {
        var result = obj is ChannelMessageRecord subject &&
            ChannelId == subject.ChannelId &&
            MessageId == subject.MessageId &&
            Date == subject.Date &&
            FromPrincipalId == subject.FromPrincipalId &&
            Message == subject.Message;

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(ChannelId, MessageId, Date, FromPrincipalId, Message);

    public static IValidator<ChannelMessageRecord> Validator { get; } = new Validator<ChannelMessageRecord>()
        .RuleFor(x => x.ChannelId).NotEmpty()
        .RuleFor(x => x.MessageId).NotEmpty()
        .RuleFor(x => x.FromPrincipalId).NotEmpty()
        .RuleFor(x => x.Message).NotEmpty()
        .Build();
}

public static class MessageRecordTool
{
    public static Option Validate(this ChannelMessageRecord subject) => ChannelMessageRecord.Validator.Validate(subject).ToOptionStatus();
}
