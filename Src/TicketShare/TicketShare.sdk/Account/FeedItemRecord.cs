using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketShare.sdk.Account;

public record FeedItemRecord
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public DateTime TimeStamp { get; init; } = DateTime.UtcNow;
}
