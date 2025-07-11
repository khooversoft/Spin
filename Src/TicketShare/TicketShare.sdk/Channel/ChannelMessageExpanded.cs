using System.Collections.Frozen;
using Toolbox.Graph.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public record ChannelMessageExpanded
{
    public string ChannelId { get; init; } = null!;
    public string MessageId { get; init; } = null!;
    public DateTime Date { get; init; } = DateTime.UtcNow;
    public string FromPrincipalId { get; init; } = null!;
    public string Message { get; init; } = null!;
    public IReadOnlyDictionary<string, string> Links = FrozenDictionary<string, string>.Empty;

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
            TsConstants.SystemTicketGroup => "Ticket Group",
            var v => await lookupUser(),
        };

        var result = new ChannelMessageExpanded
        {
            ChannelId = subject.ChannelId,
            MessageId = subject.MessageId,
            Date = subject.Date,
            FromPrincipalId = subject.FromPrincipalId,
            Message = subject.Message,
            Links = subject.Links.ToFrozenDictionary(),
            UserName = userName.NotEmpty(),
            DateAsString = subject.Date.ToString("yyyy-MM-dd HH:mm").NotEmpty(),
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
