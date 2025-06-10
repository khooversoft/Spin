using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;

public class TmEventHandler : IHybridCacheProvider
{
    private readonly ILogger<TmEventHandler> _logger;
    private readonly TicketMasterClient _ticketMasterClient;
    private readonly TicketOption _ticketOption;

    public TmEventHandler(TicketMasterClient ticketMasterClient, TicketOption ticketOption, ILogger<TmEventHandler> logger)
    {
        _ticketMasterClient = ticketMasterClient;
        _ticketOption = ticketOption;
        _logger = logger.NotNull();
    }

    public string Name => throw new NotImplementedException();

    public HybridCacheCounters Counters => throw new NotImplementedException();

    public Task<Option> Delete(string key, ScopeContext context)
    {
        throw new NotImplementedException();
    }

    public Task<Option<string>> Exists(string key, ScopeContext context)
    {
        throw new NotImplementedException();
    }

    public Task<Option<T>> Get<T>(string key, ScopeContext context)
    {
        throw new NotImplementedException();
    }

    public Task<Option> Set<T>(string key, T value, ScopeContext context)
    {
        throw new NotImplementedException();
    }
}
