//using Toolbox.Tools;
//using Toolbox.Types;

//namespace TicketShare.sdk;

//public record MessageItemRecord
//{
//    public string MessageId { get; init; } = Guid.NewGuid().ToString();
//    public string FromPrincipalId { get; init; } = null!;
//    public string ToPrincipalId { get; init; } = null!;
//    public string Message { get; init; } = null!;
//    public string? ProposalId { get; init; }
//    public DateTime? ReadDate { get; init; }

//    public static IValidator<MessageItemRecord> Validator { get; } = new Validator<MessageItemRecord>()
//        .RuleFor(x => x.MessageId).NotEmpty()
//        .RuleFor(x => x.FromPrincipalId).NotEmpty()
//        .RuleFor(x => x.ToPrincipalId).NotEmpty()
//        .RuleFor(x => x.Message).NotEmpty()
//        .Build();
//}

//public static class MessageRecordTool
//{
//    public static Option Validate(this MessageItemRecord subject) => MessageItemRecord.Validator.Validate(subject).ToOptionStatus();

//    public static bool Validate(this MessageItemRecord subject, out Option result)
//    {
//        result = subject.Validate();
//        return result.IsOk();
//    }
//}