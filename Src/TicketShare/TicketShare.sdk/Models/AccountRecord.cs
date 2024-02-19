using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketShare.sdk;

public record AccountRecord
{
    public string AccountId { get; init; } = null!;
    public string Username { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string? TextNumber { get; init; }

    public IReadOnlyList<CalendarRecord> CalendarItems { get; init; } = Array.Empty<CalendarRecord>();
    public IReadOnlyList<TicketRecord> OwnedTickets { get; init; } = Array.Empty<TicketRecord>();
}


