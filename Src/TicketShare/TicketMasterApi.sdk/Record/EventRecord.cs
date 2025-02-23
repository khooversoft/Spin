using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Types;

namespace TicketMasterApi.sdk;

public record EventRecord
{
    public string Id { get; init; } = null!;
    public DateTime? LocalDate { get; init; }
    public string? Timezone { get; init; }
    public IReadOnlyList<PromoterRecord> Promoters { get; init; } = Array.Empty<PromoterRecord>();
    public string? SeatMapUrl { get; init; }
    public ClassificationRecord? Classification { get; init; }
    public IReadOnlyList<VenueRecord> Venues { get; init; } = Array.Empty<VenueRecord>();
    public IReadOnlyList<AttractionRecord> Attractions { get; init; } = Array.Empty<AttractionRecord>();
}

public record ClassificationRecord
{
    public string? Segment { get; init; }
    public string? Genre { get; init; }
    public string? SubGenre { get; init; }
}
