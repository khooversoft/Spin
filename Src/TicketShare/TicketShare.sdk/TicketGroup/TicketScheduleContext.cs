using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;
using TicketApi.sdk;
using Toolbox.Types;
using System.Collections.Immutable;
using Toolbox.Extensions;
using Microsoft.Extensions.Logging;

namespace TicketShare.sdk;

public class TicketScheduleContext
{
    private readonly string _ticketGroupId;
    private readonly TicketSearchClient _ticketSearchClient;
    private readonly ILogger<TicketScheduleContext> _logger;
    private ClassificationRecord? _classificationRecord;
    private EventCollectionRecord? _eventCollectionRecord;

    public TicketScheduleContext(string ticketGroupId, TicketSearchClient ticketSearchClient, ILogger<TicketScheduleContext> logger)
    {
        _ticketGroupId = ticketGroupId.NotEmpty();
        _ticketSearchClient = ticketSearchClient.NotNull();
        _logger = logger.NotNull();
    }

    public string TicketGroupId => _ticketGroupId;

    public async Task<Option> Load(ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Loading TicketScheduleContext for ticketGroupId={ticketGroupId}", _ticketGroupId);

        var segmentOption = await GetSegments(context).ConfigureAwait(false);
        if (segmentOption.IsError()) return segmentOption;

        var eventOption = await GetEvents(context).ConfigureAwait(false);
        if (eventOption.IsError()) return eventOption;

        return StatusCode.OK;
    }

    public SegmentRecord? Segment { get; private set; } = null!;
    public GenreRecord? Genre { get; private set; } = null!;
    public SubGenreRecord? SubGenre { get; private set; } = null!;
    public EventAttractionRecord? Team { get; private set; } = null!;
    public IReadOnlyList<EventRecord> Events { get; private set; } = [];


    public void SetSegment(string? id)
    {
        _classificationRecord.NotNull("Classification record must be loaded before getting segments");

        Segment = (id, Segment) switch
        {
            (string v, SegmentRecord r) when v == r.Id => null,
            _ => _classificationRecord.Segements.FirstOrDefault(x => x.Id == id),
        };

        Genre = null;
        SubGenre = null;
        Team = null;
        Events = [];
    }

    public void SetGenre(string? id)
    {
        Segment.NotNull("Segment must be set before setting Genre");

        Genre = (id, Genre) switch
        {
            (string v, GenreRecord r) when v == r.Id => null,
            _ => Segment.Genres.FirstOrDefault(x => x.Id == id),
        };

        SubGenre = null;
        Team = null;
        Events = [];
    }

    public void SetSubGenre(string? id)
    {
        Genre.NotNull("Genre must be set before setting SubGenre");

        SubGenre = (id, SubGenre) switch
        {
            (string v, SubGenreRecord r) when v == r.Id => null,
            _ => Genre.SubGenres.FirstOrDefault(x => x.Id == id),
        };

        Team = null;
        Events = [];
    }

    public void SetTeam(string id)
    {
        SubGenre.NotNull("SubGenre must be set before setting Team");

        Team = (id, Team) switch
        {
            (string v, EventAttractionRecord r) when v == r.Id => null,
            _ => GetTeams().FirstOrDefault(x => x.Id == id),
        };
    }

    public void SetEvent(string id)
    {
        Team.NotNull("Team must be set before setting Event");

        Events = Events
            .Where(x => x.Id != id)
            .Append(GetEvents().FirstOrDefault(x => x.Id == id))
            .ToArray()!;
    }

    public void ClearEvent(string id)
    {
        Team.NotNull("Team must be set before clearing Event");

        Events = Events
            .Where(x => x.Id != id)
            .ToArray()!;
    }

    public IReadOnlyList<SegmentRecord> GetSegmentSelect() => (_classificationRecord?.Segements ?? [])
        .Where(x => Segment == null || x.Id == Segment?.Id)
        .OrderBy(x => x.Name)
        .ToArray();

    public IReadOnlyList<GenreRecord> GetGenreSelect() => (_classificationRecord?.Segements ?? [])
        .Where(x => x.Id == Segment?.Id)
        .SelectMany(x => x.Genres)
        .Where(x => Genre == null || x.Id == Genre?.Id)
        .OrderBy(x => x.Name)
        .ToArray();

    public IReadOnlyList<SubGenreRecord> GetSubGenreSelect() => (_classificationRecord?.Segements ?? [])
        .Where(x => x.Id == Segment?.Id)
        .SelectMany(x => x.Genres)
        .Where(x => x.Id == Genre?.Id)
        .SelectMany(x => x.SubGenres)
        .Where(x => SubGenre == null || x.Id == SubGenre?.Id)
        .OrderBy(x => x.Name)
        .ToArray();

    public IReadOnlyList<EventAttractionRecord> GetTeamSelect() => GetTeams()
        .Where(x => Team == null || x.Id == Team?.Id)
        .OrderBy(x => x.Name)
        .ToArray();

    public IReadOnlyList<(EventRecord eventRecord, bool selected)> GetEventSelect() => GetEvents()
        .Select(x => (eventRecord: x, selected: Events.Any(y => x.Id == y.Id)))
        .OrderBy(x => x.eventRecord.Name)
        .ToArray();

    public IReadOnlyList<EventAttractionRecord> GetTeams() => (_eventCollectionRecord?.Events ?? [])
        .Where(x => x.ClassificationRecord.SubGenre.Id == SubGenre?.Id && x.ClassificationRecord.SubType.Name.EqualsIgnoreCase("Team"))
        .SelectMany(x => x.Attractions)
        .ToArray();

    public IReadOnlyList<EventRecord> GetEvents() => (_eventCollectionRecord?.Events ?? [])
        .Where(x => x.ClassificationRecord.SubGenre.Id == SubGenre?.Id && x.ClassificationRecord.SubType.Name.EqualsIgnoreCase("Team"))
        .Where(x => x.Attractions.Any(a => a.Id == Team?.Id))
        .ToArray();

    private async Task<Option> GetSegments(ScopeContext context)
    {
        if (_classificationRecord == null)
        {
            var findOption = await _ticketSearchClient.GetClassifications(context).ConfigureAwait(false);
            if (findOption.IsError()) return findOption.ToOptionStatus();

            _classificationRecord = findOption.Return();
        }

        return StatusCode.OK;
    }

    private async Task<Option> GetEvents(ScopeContext context)
    {
        if (_eventCollectionRecord == null)
        {
            var findOption = await _ticketSearchClient.GetEvents(context).ConfigureAwait(false);
            if (findOption.IsError()) return findOption.ToOptionStatus();

            _eventCollectionRecord = findOption.Return();
        }

        return StatusCode.OK;
    }
}
