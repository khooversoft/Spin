using TicketShare.sdk;
using Toolbox.Email;
using Toolbox.Graph.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public readonly struct EmailMessage
{
    public EmailMessage(EmailAddress toEmail, string subject, string htmlBody)
    {
        ToEmail = toEmail;
        Subject = subject.NotEmpty();
        HtmlBody = htmlBody.NotEmpty();
    }

    public EmailAddress ToEmail { get; }
    public string Subject { get; }
    public string HtmlBody { get; }

    public override string ToString() => $"ToEmail={ToEmail}, Subject={Subject}";

    public static IValidator<EmailMessage> Validator => new Validator<EmailMessage>()
        .RuleFor(x => x.ToEmail).Validate(EmailAddress.Validator)
        .RuleFor(x => x.Subject).NotEmpty()
        .RuleFor(x => x.HtmlBody).NotNull()
        .Build();
}

internal static class EmailMessageExtensions
{
    public static Option Validate(this EmailMessage subject) => EmailMessage.Validator.Validate(subject).ToOptionStatus();
}
