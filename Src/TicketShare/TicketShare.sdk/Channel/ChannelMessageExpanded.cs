using Toolbox.Graph.Extensions;
using Toolbox.Types;

namespace TicketShare.sdk;

public record ChannelMessageExpanded
{
    public string ChannelId { get; init; } = null!;
    public string MessageId { get; init; } = null!;
    public DateTime Date { get; init; } = DateTime.UtcNow;
    public string FromPrincipalId { get; init; } = null!;
    public string Topic { get; init; } = null!;
    public string Message { get; init; } = null!;

    public string UserName { get; init; } = null!;
    public string DateAsString { get; init; } = null!;
}


public static class ChannelMessageExpandedTool
{
    public static async Task<ChannelMessageExpanded> Expand(this ChannelMessage subject, IdentityClient identityClient, ScopeContext context)
    {
        string userName = subject.FromPrincipalId switch
        {
            TsConstants.SystemIdentityEmail => "Email Notifications",
            var v => await lookupUser(),
        };

        var result = new ChannelMessageExpanded
        {
            ChannelId = subject.ChannelId,
            MessageId = subject.MessageId,
            Date = subject.Date,
            FromPrincipalId = subject.FromPrincipalId,
            Topic = subject.Topic,
            Message = subject.Message,
            UserName = userName,
            DateAsString = subject.Date.ToString("yyyy-MM-dd HH:mm"),
        };

        return result;

        async Task<string> lookupUser()
        {
            var principalOption = await identityClient.GetByPrincipalId(subject.FromPrincipalId, context);

            string userName = principalOption.IsOk() switch
            {
                true => principalOption.Return().Name ?? subject.FromPrincipalId,
                false => subject.FromPrincipalId,
            };

            return userName;
        }
    }
}
