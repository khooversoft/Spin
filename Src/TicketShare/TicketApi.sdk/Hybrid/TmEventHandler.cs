using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;

public class TmEventHandler : IDataProvider
{
    private readonly ILogger<TmEventHandler> _logger;
    private readonly TmEventClient _eventClient;
    private readonly TicketOption _ticketOption;

    public TmEventHandler(TmEventClient eventClient, TicketOption ticketOption, ILogger<TmEventHandler> logger)
    {
        _eventClient = eventClient;
        _ticketOption = ticketOption;
        _logger = logger.NotNull();
    }

    public string Name => throw new NotImplementedException();

    public DataClientCounters Counters => new DataClientCounters();


    public Task<Option> Delete(string key, ScopeContext context) => new Option(StatusCode.OK).ToTaskResult();
    public Task<Option<string>> Exists(string key, ScopeContext context) => new Option<string>(StatusCode.NotFound).ToTaskResult();

    public async Task<Option<T>> Get<T>(string key, ScopeContext context)
    {
        var eventOption = await _eventClient.GetEvents(search, context);
        if (eventOption.IsError()) return eventOption.ToOptionStatus<T>();

        return eventOption.Return().Cast<T>();
    }

    public Task<Option> Set<T>(string key, T value, ScopeContext context) => new Option(StatusCode.Conflict).ToTaskResult();

}
