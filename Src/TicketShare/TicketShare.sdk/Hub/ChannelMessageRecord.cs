using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public sealed record ChannelMessageRecord
{
    public string ChannelId { get; init; } = null!;
    public string MessageId { get; init; } = Guid.NewGuid().ToString();
    public string FromPrincipalId { get; init; } = null!;
    public string Message { get; init; } = null!;
    public string? ProposalId { get; init; }

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

    public static bool Validate(this ChannelMessageRecord subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}