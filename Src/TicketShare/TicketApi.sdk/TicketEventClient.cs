using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using TicketApi.sdk.Model;
using Toolbox.Extensions;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;

public readonly struct EventResult
{
    public IReadOnlyList<EventRecord> Events { get; init; }
    public IReadOnlyList<VenueRecord> Venues { get; init; }
    public IReadOnlyList<ImageRecord> Images { get; init; }
}

public class TicketEventClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<TicketEventClient> _logger;
    private readonly TicketOption _ticketOption;
    private readonly MonitorRate _monitorRate;

    public TicketEventClient(HttpClient client, TicketOption ticketOption, MonitorRate monitorRate, ILogger<TicketEventClient> logger)
    {
        _client = client.NotNull();
        _ticketOption = ticketOption.NotNull();
        _logger = logger.NotNull();
        _monitorRate = monitorRate;
    }

    public async Task<Option<EventResult>> GetEvents(IEnumerable<string> attractionIds, ScopeContext context)
    {
        context = context.With(_logger);

        var searchs = attractionIds.NotNull()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(x => new TicketMasterSearch(TicketSearchType.Attraction, _ticketOption, nameof(TicketEventClient)) { AttractionId = x, Size = 200, Page = 0 })
            .ToArray();

        var sequence = new Sequence<EventResult>();

        foreach (var item in searchs)
        {
            var getResult = await GetEvents(item, context);
            if (getResult.IsError()) return getResult.ToOptionStatus<EventResult>();

            sequence += getResult.Return();
        }

        var result = new EventResult
        {
            Events = sequence.SelectMany(x => x.Events).GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase).Select(x => x.First()).ToArray(),
            Venues = sequence.SelectMany(x => x.Venues).GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase).Select(x => x.First()).ToArray(),

            Images = sequence.SelectMany(x => x.Images)
                .Where(x => _ticketOption.IsImageSelected(x))
                .GroupBy(x => x.Url, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First()).ToArray(),
        };

        return result;
    }

    public async Task<Option<IReadOnlyList<EventResult>>> GetEvents(TicketMasterSearch search, ScopeContext context)
    {
        context = context.With(_logger);

        var sequenceOption = await GetEventModels(search, context);
        if (sequenceOption.IsError()) return sequenceOption.ToOptionStatus<IReadOnlyList<EventResult>>();
        var sequence = sequenceOption.Return();

        var result = sequence
            .Select(ConvertToRecord)
            .ToImmutableArray();

        return result;
    }

    public async Task<Option<IReadOnlyList<Event_EventRecordModel>>> GetEventModels(TicketMasterSearch search, ScopeContext context)
    {
        context = context.With(_logger);
        var sequence = new Sequence<Event_EventRecordModel>();

        while (true)
        {
            string query = search.Build();
            string url = $"{_ticketOption.EventUrl}?{query}";

            await _monitorRate.RecordEventAsync(context.CancellationToken);

            var model = await new RestClient(_client)
                .SetPath(url)
                .GetAsync(context)
                .GetContent<EventMasterModel>();

            if (model.IsError()) return model.ToOptionStatus<IReadOnlyList<Event_EventRecordModel>>();
            var ticketModel = model.Return();
            if (ticketModel._embedded == null) break;

            context.LogTrace("GetEvents, search={search}, page={page}, size={size}, total={total}", search, ticketModel.Page.Number, ticketModel.Page.Size, ticketModel.Page.TotalElements);
            sequence += ticketModel._embedded.Events;

            search = search with { Page = search.Page + 1 };
        }

        return sequence.ToImmutableArray();
    }

    private EventResult ConvertToRecord(Event_EventRecordModel subject)
    {
        subject.NotNull();

        //(var segement, var grene, var subGrene) = subject.Classifications.FirstOrDefault() switch
        //{
        //    null => (null, null, null),
        //    var v => (v.Segment?.ConvertTo(), v.Genre?.ConvertTo(), v.SubGenre?.ConvertTo()),
        //};

        //bool createClassification = segement != null || grene != null || subGrene != null;

        var venues = subject._embedded?.Venues?.Select(x => x.ConvertTo()).ToArray() ?? Array.Empty<VenueRecord>();
        venues.Assert(x => x.Length > 0, "No venues");
        VenueRecord venue = venues.First();

        var eventRecord = new EventRecord
        {
            Id = subject.Id,
            Name = subject.Name.NotEmpty(),
            LocalDateTime = (subject.Dates?.Start?.LocalDate, subject.Dates?.Start?.LocalTime) switch
            {
                (DateOnly date, TimeOnly time) => date.ToDateTime(time),
                (DateOnly date, null) => new DateTime(date.Year, date.Month, date.Day),
                _ => null,
            },
            Timezone = subject.Dates?.Timezone,
            SeatMapUrl = subject.Seatmap?.StaticUrl,
            VenueId = venue.Id,
            AttractionIds = subject._embedded?.Attractions?.Select(x => x.Id)?.Join(',') ?? string.Empty,
        };

        var result = new EventResult
        {
            Events = [eventRecord],
            Venues = [venue],
            Images = subject.Images.Select(x => x.ConvertTo(subject.Id)).ToArray(),
        };

        return result;
    }
}
