using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Channels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Toolbox.Email;
using Toolbox.Extensions;
using Toolbox.Graph.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class VerifyEmail
{
    private readonly UserManager<PrincipalIdentity> _userManager;
    private readonly NavigationManager _navigationManager;
    private readonly Channel<EmailMessage> _channel;
    private readonly MessageSender _messageSender;
    private readonly ILogger<VerifyEmail> _logger;

    public VerifyEmail(
        UserManager<PrincipalIdentity> userManager,
        NavigationManager navigationManager,
        Channel<EmailMessage> channel,
        MessageSender messageSender,
        ILogger<VerifyEmail> logger
        )
    {
        _userManager = userManager.NotNull();
        _navigationManager = navigationManager.NotNull();
        _channel = channel.NotNull();
        _messageSender = messageSender.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Send(PrincipalIdentity principalIdentity, string toPage, ScopeContext context)
    {
        context = context.With(_logger);

        if ((await GenerateCallbackUrl(principalIdentity, toPage, context).ConfigureAwait(false)).OutValue(out var callbackUrl).IsError(out var r1)) return r1;

        var toEmail = new EmailAddress(principalIdentity.Name.NotEmpty(), principalIdentity.Email.NotEmpty());

        var properties = new[]
        {
            new KeyValuePair<string, string>("userName", toEmail.Name),
            new KeyValuePair<string, string>("confirmationLink", callbackUrl),
        };

        var template = new TemplateFormatter("VerifyEmail.html", properties);
        string html = template.Build();

        EmailMessage emailMessage = new(toEmail, "Confirm your email", html);
        await _channel.Writer.WriteAsync(emailMessage).ConfigureAwait(false);

        // Send message to user about this
        var sendResult = await SendMessage(principalIdentity.PrincipalId, emailMessage, context).ConfigureAwait(false);
        return sendResult;
    }

    public async Task<Option<string>> GenerateCallbackUrl(PrincipalIdentity principalIdentity, string toPage, ScopeContext context)
    {
        principalIdentity.NotNull();
        toPage.NotEmpty();

        string code = await _userManager.GenerateEmailConfirmationTokenAsync(principalIdentity).ConfigureAwait(false);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = _navigationManager.GetUriWithQueryParameters(
            _navigationManager.ToAbsoluteUri(toPage).AbsoluteUri,
            new Dictionary<string, object?> { ["userId"] = principalIdentity.PrincipalId, ["code"] = code }
            );

        return HtmlEncoder.Default.Encode(callbackUrl);
    }

    public async Task<Option> ConfirmEmail(string userId, string code, ScopeContext context)
    {
        userId.NotEmpty();
        code.NotEmpty();
        context = context.With(_logger);

        PrincipalIdentity? principalIdentity = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);
        if (principalIdentity == null)
        {
            context.LogError("User not found, userId={userId}");
            return StatusCode.NotFound;
        }

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await _userManager.VerifyUserTokenAsync(principalIdentity, "Default", "EmailConfirmation", code).ConfigureAwait(false);
        if (!result)
        {
            context.LogError("Failed to verify email token, userId={userId}, code={code}");
            return StatusCode.BadRequest;
        }

        principalIdentity = principalIdentity with { EmailConfirmed = true };
        await _userManager.UpdateAsync(principalIdentity).ConfigureAwait(false);
        context.LogInformation("Email confirmed, userId={userId}", userId);

        return StatusCode.OK;
    }

    private async Task<Option> SendMessage(string principalId, EmailMessage emailMessage, ScopeContext context)
    {
        var properties = new[]
{
            new KeyValuePair<string, string>("ResendEmailLink", "/Account/ResendEmailConfirmation"),
        };

        var template = new TemplateFormatter("VerifyEmail.Txt", properties);
        var message = template.Build();

        var channelMessage = new ChannelMessage
        {
            ChannelId = IdentityTool.ToNodeKey(principalId),
            FromPrincipalId = TsConstants.SystemIdentityEmail,
            Topic = "Request for email conformation",
            Message = message,
            FilterType = TsConstants.EmailRequest
        };

        return await _messageSender.Send(channelMessage, context).ConfigureAwait(false);
    }
}
