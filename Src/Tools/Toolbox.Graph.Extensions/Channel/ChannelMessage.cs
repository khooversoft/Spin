using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions;

public sealed record ChannelMessage
{
    // If prefix "user:*"
    public string ChannelId { get; init; } = null!;
    public string MessageId { get; init; } = SequenceTool.GenerateId();
    public DateTime Date { get; init; } = DateTime.UtcNow;
    public string FromPrincipalId { get; init; } = null!;
    public string Topic { get; init; } = null!;
    public string Message { get; init; } = null!;
    public string? FilterType { get; init; }
    public DateTime? Cleared { get; init; }

    public bool Equals(ChannelMessage? obj)
    {
        var result = obj is ChannelMessage subject &&
            ChannelId == subject.ChannelId &&
            MessageId == subject.MessageId &&
            Date == subject.Date &&
            FromPrincipalId == subject.FromPrincipalId &&
            Topic == subject.Topic &&
            Message == subject.Message &&
            FilterType == subject.FilterType &&
            Cleared == subject.Cleared;

        return result;
    }

    public override int GetHashCode() => HashCode.Combine(ChannelId, MessageId, Date, FromPrincipalId, Message);

    public static IValidator<ChannelMessage> Validator { get; } = new Validator<ChannelMessage>()
        .RuleFor(x => x.ChannelId).NotEmpty()
        .RuleFor(x => x.MessageId).NotEmpty()
        .RuleFor(x => x.FromPrincipalId).NotEmpty()
        .RuleFor(x => x.Topic).NotEmpty()
        .RuleFor(x => x.Message).NotEmpty()
        .RuleFor(x => x.Cleared).ValidDateTimeOption()
        .Build();
}

public static class ChannelMessageTool
{
    public static Option Validate(this ChannelMessage subject) => ChannelMessage.Validator.Validate(subject).ToOptionStatus();

    public static ChannelIdComparer Comparer { get; } = new ChannelIdComparer();

    public static bool IsPrincipalMessage(this ChannelMessage subject) => IdentityTool.IsIdentity(subject.ChannelId);
}


public class ChannelIdComparer : IEqualityComparer<ChannelMessage>
{
    public bool Equals(ChannelMessage? x, ChannelMessage? y) =>
        x != null && y != null &&
        x.MessageId == y.MessageId;

    public int GetHashCode(ChannelMessage obj) => HashCode.Combine(obj.MessageId);
}