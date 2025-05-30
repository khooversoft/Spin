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

namespace TicketShare.sdk;

public class TicketScheduleContext
{
    private readonly string _ticketGroupId;
    private readonly TicketSearchClient _ticketSearchClient;
    private ClassificationRecord? _classificationRecord;

    public TicketScheduleContext(string ticketGroupId, TicketSearchClient ticketSearchClient)
    {
        _ticketGroupId = ticketGroupId.NotEmpty();
        _ticketSearchClient = ticketSearchClient.NotNull();
    }

    public string TicketGroupId => _ticketGroupId;

    public SegmentRecord? Segment { get; private set; } = null!;
    public GenreRecord? Genre { get; private set; } = null!;
    public SubGenreRecord? SubGenre { get; private set; } = null!;


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
    }

    public void SetSubGenre(string? id)
    {
        Genre.NotNull("Genre must be set before setting SubGenre");

        SubGenre = (id, SubGenre) switch
        {
            (string v, SubGenreRecord r) when v == r.Id => null,
            _ => Genre.SubGenres.FirstOrDefault(x => x.Id == id),
        };
    }

    public async Task<Option<IReadOnlyList<SegmentRecord>>> GetSegments(ScopeContext context)
    {
        if (_classificationRecord == null)
        {
            var findOption = await _ticketSearchClient.GetClassifications(context).ConfigureAwait(false);
            if (findOption.IsError()) return findOption.ToOptionStatus<IReadOnlyList<SegmentRecord>>();

            _classificationRecord = findOption.Return();
        }

        return _classificationRecord.Segements.ToOption();
    }

    public IReadOnlyList<SegmentRecord> GetSegmentSelect()
    {
        var result = (_classificationRecord?.Segements ?? Array.Empty<SegmentRecord>())
            .Where(x => Segment == null || x.Id == Segment?.Id)
            .OrderBy(x => x.Name)
            .ToArray();

        return result;
    }

    public IReadOnlyList<GenreRecord> GetGenreSelect()
    {
        var result = (_classificationRecord?.Segements ?? Array.Empty<SegmentRecord>())
            .Where(x => x.Id == Segment?.Id)
            .SelectMany(x => x.Genres)
            .Where(x => Genre == null || x.Id == Genre?.Id)
            .OrderBy(x => x.Name)
            .ToArray();

        return result;
    }

    public IReadOnlyList<SubGenreRecord> GetSubGenreSelect()
    {
        var result = (_classificationRecord?.Segements ?? Array.Empty<SegmentRecord>())
            .Where(x => x.Id == Segment?.Id)
            .SelectMany(x => x.Genres)
            .Where(x => x.Id == Genre?.Id)
            .SelectMany(x => x.SubGenres)
            .Where(x => SubGenre == null || x.Id == SubGenre?.Id)
            .OrderBy(x => x.Name)
            .ToArray();

        return result;
    }


}
