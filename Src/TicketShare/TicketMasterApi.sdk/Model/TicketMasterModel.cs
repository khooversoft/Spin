using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketMasterApi.sdk;

public record TicketMasterModel
{
    public RootEmbedded _embedded { get; init; } = null!;
    public LinkModel? _links { get; init; }
    public PageModel? Page { get; init; }
}

public record RootEmbedded
{
    public IReadOnlyList<EventRecordModel> Events { get; init; } = Array.Empty<EventRecordModel>();
}

public record LinkModel
{
    public HrefModel? First { get; init; }
    public HrefModel? Self { get; init; }
    public HrefModel? Next { get; init; }
    public HrefModel? Last { get; init; }
}

public record HrefModel
{
    public string? Href { get; init; }
}

public record PageModel
{
    public int? Size { get; init; }
    public int? TotalElements { get; init; }
    public int? TotalPages { get; init; }
    public int? Number { get; init; }
}
