using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class TicketGroupManager
{
    private readonly UserAccountManager _userAccountManager;
    private readonly TicketGroupClient _ticketGroupClient;

    public TicketGroupManager(UserAccountManager userAccountManager, TicketGroupClient ticketGroupClient)
    {
        _userAccountManager = userAccountManager.NotNull();
        _ticketGroupClient = ticketGroupClient.NotNull();
    }

    public TicketGroupContext GetContext(string ticketGroupId) => new TicketGroupContext(ticketGroupId, _ticketGroupClient);

    public async Task<Option<IReadOnlyList<TicketGroupModel>>> GetTicketGroups(ScopeContext context)
    {
        string principalId = await _userAccountManager.GetPrincipalId().ConfigureAwait(false);

        var result = await Search(principalId, context).ConfigureAwait(false);
        if (result.IsError()) return result.ToOptionStatus<IReadOnlyList<TicketGroupModel>>();

        var list = result.Return().Select(x => x.ConvertTo()).ToImmutableArray();
        return list;
    }

    public async Task<Option<string>> Create(TicketGroupHeaderModel ticketGroupHeader, ScopeContext context)
    {
        string principalId = await _userAccountManager.GetPrincipalId().ConfigureAwait(false);

        var ticketGroupRecord = ticketGroupHeader.ConvertTo().ConvertTo()
            .SetTicketGroupId(principalId)
            .SetChannelId()
            .SetOwner(principalId);

        context.LogInformation("Creating TicketGroup ticket group ticketGroupId= name={name}", ticketGroupHeader.Name);

        var option = await _ticketGroupClient.Add(ticketGroupRecord, context).ConfigureAwait(false);
        option.LogStatus(context, "Create TicketGroup, name={name}", [ticketGroupHeader.Name]);
        if (option.IsError())
        {
            return (StatusCode.Conflict, $"Ticket group with name {ticketGroupHeader.Name} already exists");
        }

        context.LogInformation("Creating HubChannel channelId={channelId} for ticket group name={name}", ticketGroupRecord.ChannelId, ticketGroupHeader.Name);
        return ticketGroupRecord.TicketGroupId;
    }

    public Task<Option<IReadOnlyList<TicketGroupRecord>>> Search(string principalId, ScopeContext context) => _ticketGroupClient.Search(principalId, context);
}
