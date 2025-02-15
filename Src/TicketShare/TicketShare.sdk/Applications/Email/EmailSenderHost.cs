using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Toolbox.Email;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class EmailSenderHost : ChannelReceiverHost<EmailMessage>
{
    private readonly IEmailWriter _emailSender;
    private readonly ILogger<EmailSenderHost> _logger;

    public EmailSenderHost(Channel<EmailMessage> channel, IEmailWriter emailSender, ILogger<EmailSenderHost> logger)
        : base(nameof(EmailSenderHost), channel, logger)
    {
        _emailSender = emailSender.NotNull();
        _logger = logger.NotNull();
    }

    protected override async Task<Option> ProcessMessage(EmailMessage message, ScopeContext context)
    {
        context = context.With(_logger);

        var result = await _emailSender.WriteHtml([message.ToEmail], message.Subject, message.HtmlBody, context).ConfigureAwait(false);
        if (result.IsError(out var r)) return r.LogStatus(context, nameof(EmailSenderHost));

        context.LogTrace("Sent email message to process, message={message}", message);
        return result;
    }
}
