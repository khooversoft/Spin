using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class TicketGroupManager
{
    private readonly UserAccountManager _userAccountManager;
    private readonly ILogger<TicketGroupManager> _logger;
    private readonly TicketGroupClient _ticketGroupClient;
    private readonly TicketGroupSearchClient _ticketGroupSearchClient;

    public TicketGroupManager(
        UserAccountManager userAccountManager,
        TicketGroupClient ticketGroupClient,
        TicketGroupSearchClient ticketGroupSearchClient,
        ILogger<TicketGroupManager> logger
        )
    {
        _userAccountManager = userAccountManager.NotNull();
        _ticketGroupClient = ticketGroupClient.NotNull();
        _ticketGroupSearchClient = ticketGroupSearchClient.NotNull();
        _logger = logger.NotNull();
    }

    public TicketGroupContext GetContext(string ticketGroupId) => new TicketGroupContext(ticketGroupId, _ticketGroupClient, _logger);

    public async Task<Option<IReadOnlyList<TicketGroupModel>>> GetTicketGroups()
    {
        var context = new ScopeContext(_logger);

        string principalId = await _userAccountManager.GetPrincipalId().ConfigureAwait(false);

        var result = await _ticketGroupSearchClient.GetByOwner(principalId, context).ConfigureAwait(false);
        if (result.IsError()) return result.ToOptionStatus<IReadOnlyList<TicketGroupModel>>();

        var list = result.Return().Select(x => x.ConvertTo()).ToImmutableArray();
        return list;
    }

    public async Task<Option> Create(TicketGroupHeaderModel ticketGroupHeader)
    {
        var context = new ScopeContext(_logger);
        string principalId = await _userAccountManager.GetPrincipalId().ConfigureAwait(false);

        var ticketGroupRecord = ticketGroupHeader.ConvertTo().ConvertTo()
            .SetTicketGroupId(principalId)
            .SetChannelId()
            .SetOwner(principalId);

        var option = await _ticketGroupClient.Add(ticketGroupRecord, context).ConfigureAwait(false);
        option.LogStatus(context, "Create TicketGroup, name={name}", [ticketGroupHeader.Name]);
        if (option.IsError())
        {
            return (StatusCode.Conflict, $"Ticket group with name {ticketGroupHeader.Name} already exists");
        }

        return option;
    }
}
