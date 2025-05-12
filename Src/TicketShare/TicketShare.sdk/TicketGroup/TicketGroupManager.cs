using System.Collections.Frozen;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Graph.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

public class TicketGroupManager
{
    private readonly UserAccountContext _userAccountContext;
    private readonly TicketGroupClient _ticketGroupClient;
    private readonly MessageSender _messageSender;
    private readonly ILogger<TicketGroupManager> _logger;

    public TicketGroupManager(
        UserAccountContext userAccountManager,
        TicketGroupClient ticketGroupClient,
        MessageSender messageSender,
        ILogger<TicketGroupManager> logger
        )
    {
        _userAccountContext = userAccountManager.NotNull();
        _ticketGroupClient = ticketGroupClient.NotNull();
        _messageSender = messageSender.NotNull();
        _logger = logger.NotNull();
    }

    public TicketGroupContext GetContext(string ticketGroupId) => new TicketGroupContext(ticketGroupId, _ticketGroupClient);

    public async Task<Option<IReadOnlyList<TicketGroupModel>>> GetTicketGroups(ScopeContext context)
    {
        context = context.With(_logger);

        var principalOption = await _userAccountContext.GetPrincipalIdentity(context).ConfigureAwait(false);
        if (principalOption.IsError()) return principalOption.ToOptionStatus<IReadOnlyList<TicketGroupModel>>();
        string principalId = principalOption.Return().PrincipalId;

        var result = await Search(principalId, context).ConfigureAwait(false);
        if (result.IsError()) return result.ToOptionStatus<IReadOnlyList<TicketGroupModel>>();

        var list = result.Return().Select(x => x.ConvertTo()).ToImmutableArray();
        return list;
    }

    public async Task<Option<string>> Create(TicketGroupHeaderModel ticketGroupHeader, ScopeContext context)
    {
        if (ticketGroupHeader.NotNull().Validate().IsError(out var r)) return r.ToOptionStatus<string>();
        context = context.With(_logger);

        var principalOption = await _userAccountContext.GetPrincipalIdentity(context).ConfigureAwait(false);
        if (principalOption.IsError()) return principalOption.ToOptionStatus<string>();
        string principalId = principalOption.Return().PrincipalId;

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

        await CreatedTicketMessage(principalId, ticketGroupHeader.Name, ticketGroupRecord.TicketGroupId, context).ConfigureAwait(false);

        context.LogInformation("Created TicketGroup ticket group ticketGroupId= name={name}", ticketGroupHeader.Name);
        return ticketGroupRecord.TicketGroupId;
    }

    public Task<Option<IReadOnlyList<TicketGroupRecord>>> Search(string principalId, ScopeContext context) => _ticketGroupClient.Search(principalId, context);

    private async Task CreatedTicketMessage(string principalId, string name, string ticketGroupId, ScopeContext context)
    {
        principalId.NotEmpty();
        name.NotEmpty();
        ticketGroupId.NotEmpty();

        var properties = new[]
{
            new KeyValuePair<string, string>("name", name),
            new KeyValuePair<string, string>("ticketGroupId", ticketGroupId),
            new KeyValuePair<string, string>("ticketGroupUri", ApplicationUri.GetTicketGroup(ticketGroupId)),
        };

        var message = new TemplateFormatter("CreatedTicketGroup.txt", properties).Build();

        var channelMessage = new ChannelMessage
        {
            ChannelId = IdentityTool.ToNodeKey(principalId),
            FromPrincipalId = TsConstants.SystemTicketGroup,
            Message = message,
            Links = new Dictionary<string, string>
            {
                ["Ticket Group"] = ApplicationUri.GetTicketGroup(ticketGroupId),
            }.ToFrozenDictionary(),
        };

        (await _messageSender.Send(channelMessage, context).ConfigureAwait(false)).LogStatus(context, nameof(CreatedTicketMessage)).ThrowOnError();
    }
}
