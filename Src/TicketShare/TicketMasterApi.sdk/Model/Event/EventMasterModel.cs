using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketMasterApi.sdk.Model.Event;

public record EventMasterModel
{
    public Event_RootEmbedded _embedded { get; init; } = null!;
    public LinkModel? _links { get; init; }
    public PageModel? Page { get; init; }
}

public record Event_RootEmbedded
{
    public IReadOnlyList<Event_EventRecordModel> Events { get; init; } = Array.Empty<Event_EventRecordModel>();
}
