using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketMasterApi.sdk;

public class TicketMasterDiscoverClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<TicketMasterDiscoverClient> _logger;
    private readonly TicketMasterOption _ticketMasterOption;

    public TicketMasterDiscoverClient(HttpClient client, TicketMasterOption ticketMasterOption, ILogger<TicketMasterDiscoverClient> logger)
    {
        _client = client.NotNull();
        _ticketMasterOption = ticketMasterOption.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<IReadOnlyList<EventRecord>>> GetEvents(TicketMasterSearch search, ScopeContext context)
    {
        var sequence = new Sequence<EventRecordModel>();

        while (true)
        {
            string query = search.GetQuery(_ticketMasterOption.ApiKey);
            string url = $"{_ticketMasterOption.DiscoveryUrl}/events?{query}";

            var model = await new RestClient(_client)
                .SetPath(url)
                .GetAsync(context.With(_logger))
                .GetContent<TicketMasterModel>();

            if (model.IsError()) return model.ToOptionStatus<IReadOnlyList<EventRecord>>();
            var ticketMasterModel = model.Return();
            if (ticketMasterModel._embedded == null) break;

            sequence += ticketMasterModel._embedded.Events;

            search = search with { Page = search.Page + 1 };
        }

        var result = sequence.Select(x => ConvertToRecord(x)).ToImmutableArray();
        return result;
    }

    private EventRecord ConvertToRecord(EventRecordModel subject)
    {
        subject.NotNull();

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
            Classification = new ClassificationRecord
            {
                Segment = subject.Classifications.FirstOrDefault()?.Segment?.Name,
                Genre = subject.Classifications.FirstOrDefault()?.Genre?.Name,
                SubGenre = subject.Classifications.FirstOrDefault()?.SubGenre?.Name,
            },
            Venues = (subject._embedded?.Venues ?? Array.Empty<VenueModel>()).Select(x => x.ConvertTo()).ToImmutableArray(),
            Attractions = (subject._embedded?.Attractions ?? Array.Empty<AttractionModel>()).Select(x => x.ConvertTo()).ToImmutableArray(),
        };

        return result;
    }
}
