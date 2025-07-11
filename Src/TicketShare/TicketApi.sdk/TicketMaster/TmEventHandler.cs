using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;

public class TmEventHandler : DataProviderBase
{
    public const string _prefixPath = "config/TicketData";
    private readonly ILogger<TmEventHandler> _logger;
    private readonly TmEventClient _eventClient;
    private readonly TicketOption _ticketOption;

    public TmEventHandler(TmEventClient eventClient, TicketOption ticketOption, ILogger<TmEventHandler> logger)
    {
        _eventClient = eventClient;
        _ticketOption = ticketOption;
        _logger = logger.NotNull();
    }

    public override Task<Option<string>> Exists(string key, ScopeContext context) => new Option<string>(StatusCode.OK).ToTaskResult();

    public override async Task<Option<T>> Get<T>(string key, object? state, ScopeContext context)
    {
        TicketMasterSearch search = state as TicketMasterSearch ?? throw new ArgumentException("State must be of type TicketMasterSearch", nameof(state));

        var eventOption = await _eventClient.GetEvents(search, context);
        if (eventOption.IsError()) return eventOption.ToOptionStatus<T>();

        return eventOption.Return().Cast<T>();
    }
}
