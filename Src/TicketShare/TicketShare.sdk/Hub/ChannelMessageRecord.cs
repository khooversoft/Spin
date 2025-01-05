using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public sealed record ChannelMessageRecord
{
    public string ChannelId { get; init; } = null!;
    public string MessageId { get; init; } = Guid.NewGuid().ToString();
    public DateTime Date { get; init; } = DateTime.UtcNow;
    public string FromPrincipalId { get; init; } = null!;
    public string Message { get; init; } = null!;
    public IReadOnlyList<MessageAction> Actions { get; init; } = Array.Empty<MessageAction>();

    public bool Equals(ChannelMessageRecord? obj)
    {
        var result = obj is ChannelMessageRecord subject &&
            ChannelId == subject.ChannelId &&
            MessageId == subject.MessageId &&
            Date == subject.Date &&
            FromPrincipalId == subject.FromPrincipalId &&
            Message == subject.Message &&
            Actions.OrderBy(x => x.ToString()).SequenceEqual(subject.Actions.OrderBy(x => x.ToString()));

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(ChannelId, MessageId, Date, FromPrincipalId, Message, Actions);

    public static IValidator<ChannelMessageRecord> Validator { get; } = new Validator<ChannelMessageRecord>()
        .RuleFor(x => x.ChannelId).NotEmpty()
        .RuleFor(x => x.MessageId).NotEmpty()
        .RuleFor(x => x.FromPrincipalId).NotEmpty()
        .RuleFor(x => x.Message).NotEmpty()
        .RuleFor(x => x.Actions).NotNull()
        .RuleForEach(x => x.Actions).Validate(MessageAction.Validator)
        .Build();
}

public sealed record MessageAction
{
    public string Type { get; init; } = null!;
    public string Command { get; init; } = null!;

    public static IValidator<MessageAction> Validator { get; } = new Validator<MessageAction>()
        .RuleFor(x => x.Type).NotEmpty()
        .RuleFor(x => x.Command).NotEmpty()
        .Build();

    public static MessageAction CreateProposal(string proposalId) => new MessageAction { Type = "proposal", Command = proposalId };
}

public static class MessageRecordTool
{
    public static Option Validate(this ChannelMessageRecord subject) => ChannelMessageRecord.Validator.Validate(subject).ToOptionStatus();
    public static Option Validate(this MessageAction subject) => MessageAction.Validator.Validate(subject).ToOptionStatus();
}
