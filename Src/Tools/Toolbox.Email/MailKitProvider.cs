using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Email;

public class MailKitProvider : IEmailWriter
{
    private readonly EmailOption _emailOption;
    private readonly ILogger<MailKitProvider> _logger;

    public MailKitProvider(EmailOption emailOption, ILogger<MailKitProvider> logger)
    {
        _emailOption = emailOption.NotNull().Action(x => x.Validate().ThrowOnError());
        _logger = logger.NotNull();
    }

    public Task<Option> WriteText(IEnumerable<EmailAddress> toEmails, string subject, string textMessage, ScopeContext context)
    {
        var textPart = new TextPart("text")
        {
            Text = textMessage
        };

        return InternalSend(toEmails, subject, textPart, context);
    }

    public Task<Option> WriteHtml(IEnumerable<EmailAddress> toEmails, string subject, string htmlText, ScopeContext context)
    {
        var textPart = new TextPart("html")
        {
            Text = htmlText
        };

        return InternalSend(toEmails, subject, textPart, context);
    }

    private async Task<Option> InternalSend(IEnumerable<EmailAddress> toEmails, string subject, TextPart text, ScopeContext context)
    {
        context = context.With(_logger);
        var toList = toEmails.NotNull().ToArray().Action(x => x.Length.Assert(x => x > 0, "No email addresses"));

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_emailOption.Name, _emailOption.Email));
        toList.ForEach(x => message.To.Add(new MailboxAddress(x.Name, x.Email)));
        message.Subject = subject;
        message.Body = text;

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailOption.Email, _emailOption.Password);

            string result = await client.SendAsync(message);
            context.LogInformation("Email sent: {result}, toEmail={toEmail}, subject={subject}", result, toEmails.Select(x => x.Email), subject);

            await client.DisconnectAsync(true);
        }

        return StatusCode.OK;
    }
}
