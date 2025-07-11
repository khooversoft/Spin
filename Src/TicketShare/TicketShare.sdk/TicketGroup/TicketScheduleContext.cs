using Microsoft.Extensions.Logging;
using TicketApi.sdk;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;


public class TicketScheduleContext
{
    private readonly string _ticketGroupId;
    private readonly TicketMasterClient _ticketSearchClient;
    private readonly ILogger<TicketScheduleContext> _logger;
    private ClassificationRecord? _classificationRecord;
    private AttractionCollectionRecord? _attractionCollectionRecord;
    private EventCollectionRecord? _eventCollectionRecord;
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    private readonly IconCollectionService _iconCollection;
    private readonly TicketOption _ticketOption;

    public TicketScheduleContext(
        string ticketGroupId,
        TicketMasterClient ticketSearchClient,
        IconCollectionService iconCollection,
        TicketOption ticketOption,
        ILogger<TicketScheduleContext> logger
        )
    {
        _ticketGroupId = ticketGroupId.NotEmpty();
        _ticketSearchClient = ticketSearchClient.NotNull();
        _iconCollection = iconCollection.NotNull();
        _ticketOption = ticketOption.NotNull();
        _logger = logger.NotNull();
    }

    public string TicketGroupId => _ticketGroupId;
    public SegmentRecord? Segment { get; private set; } = null!;
    public GenreRecord? Genre { get; private set; } = null!;
    public SubGenreRecord? SubGenre { get; private set; } = null!;
    public AttractionRecord? Team { get; private set; } = null!;
    public IReadOnlyList<EventRecordSelect> Events { get; private set; } = [];
    public IReadOnlyList<SeatModel> Seats { get; private set; } = [];

    public bool CanSave() => Segment != null && Genre != null && SubGenre != null && Team != null && Events.Any(x => x.Selected);
    public bool ShowSegment() => _classificationRecord == null || _classificationRecord.Segments.Count != 1;

    public async Task<Option> LoadSegments(ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Loading segments for ticketGroupId={ticketGroupId}", _ticketGroupId);

        await _lock.WaitAsync(context.CancellationToken).ConfigureAwait(false);
        try
        {
            if (_classificationRecord != null) return StatusCode.OK;

            var findOption = await _ticketSearchClient.GetClassifications(context).ConfigureAwait(false);
            if (findOption.IsError()) return findOption.ToOptionStatus();

            _classificationRecord = findOption.Return();
            Segment = null;
            _attractionCollectionRecord = null;
            Genre = null;
            SubGenre = null;

            if (_classificationRecord.Segments.Count == 1) SetSegment(_classificationRecord.Segments[0].Id);
            return StatusCode.OK;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Option> LoadAttractions(ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Loading attractions for ticketGroupId={ticketGroupId}", _ticketGroupId);

        await _lock.WaitAsync(context.CancellationToken).ConfigureAwait(false);
        try
        {
            if (_attractionCollectionRecord != null) return StatusCode.OK;

            _attractionCollectionRecord = null;
            Team = null;
            Events = [];

            if (Segment == null || Genre == null || SubGenre == null) return StatusCode.OK;
            var findOption = await _ticketSearchClient.GetAttractions(Segment, Genre, SubGenre, context).ConfigureAwait(false);
            if (findOption.IsError()) return findOption.ToOptionStatus();

            _attractionCollectionRecord = findOption.Return();
            _iconCollection.AddAndMerge(_attractionCollectionRecord, context);

            return StatusCode.OK;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Option> LoadEvents(ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Loading events for ticketGroupId={ticketGroupId}", _ticketGroupId);

        await _lock.WaitAsync(context.CancellationToken).ConfigureAwait(false);
        try
        {
            _eventCollectionRecord = null;
            Events = [];

            if (Team == null) return StatusCode.OK;

            var findOption = await _ticketSearchClient.GetEvents(Team, context).ConfigureAwait(false);
            if (findOption.IsError()) return findOption.ToOptionStatus();

            _eventCollectionRecord = findOption.Return();
            BuildEventSelect();
            return StatusCode.OK;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Option> SaveSchedule(ScopeContext context)
    {
        context = context.With(_logger);
        CanSave().BeTrue("Cannot save schedule, missing required fields");
        context.LogDebug("Saving TicketScheduleContext for ticketGroupId={ticketGroupId}", _ticketGroupId);

        await _lock.WaitAsync(context.CancellationToken).ConfigureAwait(false);
        try
        {
            // Here you would implement the logic to save the schedule, e.g., to a database or file.
            // This is a placeholder for the actual save logic.
            context.LogInformation("Schedule saved successfully for ticketGroupId={ticketGroupId}", _ticketGroupId);
            return StatusCode.OK;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void SetSegment(string? id)
    {
        _classificationRecord.NotNull("Classification record must be loaded before getting segments");

        Segment = (id, Segment) switch
        {
            (string v, SegmentRecord r) when v == r.Id => null,
            _ => _classificationRecord.Segments.FirstOrDefault(x => x.Id == id),
        };

        Genre = null;
        SubGenre = null;
        _attractionCollectionRecord = null;
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
        _attractionCollectionRecord = null;
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

        _attractionCollectionRecord = null;
        Team = null;
        Events = [];
    }

    public void SetTeam(string? id)
    {
        _attractionCollectionRecord.NotNull("Attractions must be loaded first");

        Team = (id, Team) switch
        {
            (string v, AttractionRecord r) when v == r.Id => null,
            _ => _attractionCollectionRecord.Attractions.FirstOrDefault(x => x.Id == id),
        };

        _eventCollectionRecord = null;
        Events = [];
    }

    public void SetEvent(string? id)
    {
        Events.NotNull("Events are not set before setting Event");

        if (id == null)
        {
            Events.ForEach(x => x.Selected = false);
            return;
        }

        Events.Where(x => x.Id == id).First().Action(x => x.Selected = !x.Selected);
    }

    public void SetSeat(SeatModel? seat) => Seats = Seats
        .Where(x => x.Id != seat?.Id) // Remove existing seat with the same Id
        .Concat(seat != null ? [seat] : []) // Append the new seat if it's not null
        .ToArray();

    public IReadOnlyList<SegmentRecord> GetSegmentSelect() => (_classificationRecord?.Segments ?? [])
        .Where(x => Segment == null || x.Id == Segment?.Id)
        .OrderBy(x => x.Name)
        .ToArray();

    public IReadOnlyList<GenreRecord> GetGenreSelect() => (_classificationRecord?.Segments ?? [])
        .Where(x => x.Id == Segment?.Id)
        .SelectMany(x => x.Genres)
        .Where(x => Genre == null || x.Id == Genre?.Id)
        .OrderBy(x => x.Name)
        .ToArray();

    public IReadOnlyList<SubGenreRecord> GetSubGenreSelect() => (_classificationRecord?.Segments ?? [])
        .Where(x => x.Id == Segment?.Id)
        .SelectMany(x => x.Genres)
        .Where(x => x.Id == Genre?.Id)
        .SelectMany(x => x.SubGenres)
        .Where(x => SubGenre == null || x.Id == SubGenre?.Id)
        .OrderBy(x => x.Name)
        .ToArray();

    public IReadOnlyList<AttractionRecord> GetTeamSelect() => (_attractionCollectionRecord?.Attractions ?? [])
        .Where(x => Team == null || x.Id == Team?.Id)
        .OrderBy(x => x.Name)
        .ToArray();

    public IReadOnlyList<EventRecord> GetEvents(bool onlyHome) => (_eventCollectionRecord?.Events?.ToArray() ?? [])
        .Where(x => !onlyHome || (Team != null && x.Attractions.FirstOrDefault()?.Id == Team.Id))
        .ToArray();

    public IReadOnlyList<EventRecordSelect> GetEventSelect() => Events;

    private void BuildEventSelect()
    {
        Events = GetEvents(_ticketOption.OnlyHomeGames)
            .Select(x => (eventRecord: x, selected: Events.Any(y => x.Id == y.Id)))
            .OrderBy(x => x.eventRecord.GetLocalDateTime())
            .Select(x => new EventRecordSelect(x.eventRecord.Id, x.selected, x.eventRecord.GetLocalDateTime(), x.eventRecord.Name))
            .ToArray();

        if (Events.Any(x => x.Selected)) System.Diagnostics.Debugger.Break();
    }
}
