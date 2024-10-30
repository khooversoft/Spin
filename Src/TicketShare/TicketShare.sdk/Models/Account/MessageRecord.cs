using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public record MessageRecord
{
    public string FromPrincipalId { get; init; } = null!;
    public string ToPrincipalId { get; init; } = null!;
    public string Message { get; init; } = null!;
    public string? ProposalId { get; init; }

    public static IValidator<MessageRecord> Validator { get; } = new Validator<MessageRecord>()
        .RuleFor(x => x.FromPrincipalId).NotEmpty()
        .RuleFor(x => x.ToPrincipalId).NotEmpty()
        .RuleFor(x => x.Message).NotEmpty()
        .Build();
}

public static class MessageRecordTool
{
    public static Option Validate(this MessageRecord subject) => MessageRecord.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this MessageRecord subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}