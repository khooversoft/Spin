using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketMasterApi.sdk;

public class TicketMasterCasheClient
{
    private TicketMasterClient _client;
    private readonly IMemoryCache _memoryCache;

    private readonly MemoryCacheEntryOptions _memoryOptions = new MemoryCacheEntryOptions
    {
        SlidingExpiration = TimeSpan.FromMinutes(30)
    };

    public TicketMasterCasheClient(HttpClient client, TicketMasterOption ticketMasterOption, IMemoryCache memoryCache, ILogger<TicketMasterClient> logger)
    {
        _client = new TicketMasterClient(client, ticketMasterOption, logger);
        _memoryCache = memoryCache.NotNull();
    }

    public async Task<Option<IReadOnlyList<PromoterEventRecord>>> GetEvents(TicketMasterSearch search, ScopeContext context)
    {
        string hash = search.GetQueryHash();
        if (_memoryCache.TryGetValue<IReadOnlyList<PromoterEventRecord>>(hash, out var data))
        {
            return data.NotNull().ToOption();
        }

        var result = await _client.GetEvents(search, context);
        if (result.IsError()) return result;

        var resultData = result.Return();
        _memoryCache.Set(hash, data, _memoryOptions);

        return result;
    }
}
