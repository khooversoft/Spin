using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Email;

public class MailKitProvider : IEmailSender
{
    private readonly EmailOption _emailOption;
    private readonly ILogger<MailKitProvider> _logger;

    public MailKitProvider(IOptions<EmailOption> emailOption, ILogger<MailKitProvider> logger)
    {
        _emailOption = emailOption.NotNull().Value.Action(x => x.Validate().ThrowOnError());
        _logger = logger.NotNull();
    }

    public Task<Option> SendText(EmailAddress toEmail, string subject, string textMessage, ScopeContext context)
    {
        var textPart = new TextPart("text")
        {
            Text = textMessage
        };

        return InternalSend(toEmail, subject, textPart, context);
    }

    public Task<Option> SendHtml(EmailAddress toEmail, string subject, string htmlText, ScopeContext context)
    {
        var textPart = new TextPart("html")
        {
            Text = htmlText
        };

        return InternalSend(toEmail, subject, textPart, context);
    }

    private async Task<Option> InternalSend(EmailAddress toEmail, string subject, TextPart text, ScopeContext context)
    {

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_emailOption.Name, _emailOption.Email));
        message.To.Add(new MailboxAddress(toEmail.Name, toEmail.Email));
        message.Subject = subject;
        message.Body = text;

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync("smtp.office365.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailOption.Email, _emailOption.Password);

            string result = await client.SendAsync(message);
            context.LogInformation("Email sent: {result}, toEmail={toEmail}, subject={subject}", result, toEmail.Email, subject);

            await client.DisconnectAsync(true);
        }

        return StatusCode.OK;
    }
}
