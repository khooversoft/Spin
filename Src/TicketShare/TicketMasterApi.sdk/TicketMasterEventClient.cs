using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TicketMasterApi.sdk.Model.Event;
using Toolbox.Extensions;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketMasterApi.sdk;

public class TicketMasterEventClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<TicketMasterEventClient> _logger;
    private readonly TicketMasterOption _ticketMasterOption;
    private readonly IMemoryCache _memoryCache;

    private readonly MemoryCacheEntryOptions _memoryOptions = new MemoryCacheEntryOptions
    {
        SlidingExpiration = TimeSpan.FromMinutes(30)
    };

    public TicketMasterEventClient(HttpClient client, TicketMasterOption ticketMasterOption, IMemoryCache memoryCache, ILogger<TicketMasterEventClient> logger)
    {
        _client = client.NotNull();
        _ticketMasterOption = ticketMasterOption.NotNull();
        _memoryCache = memoryCache.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<IReadOnlyList<EventRecord>>> GetEvents(TicketMasterSearch search, ScopeContext context)
    {
        string? hash = null;

        if (_ticketMasterOption.UseCache)
        {
            hash = search.GetQueryHash();
            if (_memoryCache.TryGetValue<IReadOnlyList<EventRecord>>(hash, out var data))
            {
                return data.NotNull().ToOption();
            }
        }

        var result = await InternalGetEvents(search, context);
        if (result.IsError()) return result;

        if (_ticketMasterOption.UseCache)
        {
            var resultData = result.Return();
            _memoryCache.Set(hash.NotEmpty(), resultData, _memoryOptions);
        }

        return result;
    }

    public async Task<Option<IReadOnlyList<EventRecord>>> InternalGetEvents(TicketMasterSearch search, ScopeContext context)
    {
        var sequence = new Sequence<Event_EventRecordModel>();

        while (true)
        {
            string query = search.GetQuery(_ticketMasterOption.ApiKey);
            string url = $"{_ticketMasterOption.EventUrl}?{query}";

            var model = await new RestClient(_client)
                .SetPath(url)
                .GetAsync(context.With(_logger))
                .GetContent<EventMasterModel>();

            if (model.IsError()) return model.ToOptionStatus<IReadOnlyList<EventRecord>>();
            var ticketMasterModel = model.Return();
            if (ticketMasterModel._embedded == null) break;

            sequence += ticketMasterModel._embedded.Events;

            search = search with { Page = search.Page + 1 };
        }

        var result = sequence.Select(x => ConvertToRecord(x)).ToImmutableArray();
        return result;
    }

    private EventRecord ConvertToRecord(Event_EventRecordModel subject)
    {
        subject.NotNull();

        var segement = subject.Classifications.FirstOrDefault()?.Segment?.ConvertTo();
        var grene = subject.Classifications.FirstOrDefault()?.Genre?.ConvertTo();
        var subGrene = subject.Classifications.FirstOrDefault()?.SubGenre?.ConvertTo();
        bool createClassification = segement != null || grene != null || subGrene != null;

        var result = new EventRecord
        {
            Id = subject.Id,
            LocalDate = (subject.Dates?.Start?.LocalDate, subject.Dates?.Start?.LocalTime) switch
            {
                (DateOnly date, TimeOnly time) => date.ToDateTime(time),
                (DateOnly date, null) => new DateTime(date.Year, date.Month, date.Day),
                _ => null,
            },
            Timezone = subject.Dates?.Timezone,
            Promoters = subject.Promoters.Select(x => x.ConvertTo()).ToImmutableArray(),
            SeatMapUrl = subject.Seatmap?.StaticUrl,
            Classification = createClassification ? new ClassificationRecord
            {
                Segement = segement.NotNull(),
                Grene = grene.NotNull(),
                SubGrene = subGrene.NotNull(),
            } : null,
            Venues = (subject._embedded?.Venues ?? Array.Empty<Event_VenueModel>()).Select(x => x.ConvertTo()).ToImmutableArray(),
            Attractions = (subject._embedded?.Attractions ?? Array.Empty<Event_AttractionModel>()).Select(x => x.ConvertTo()).ToImmutableArray(),
        };

        return result;
    }
}
