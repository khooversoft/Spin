using System.Collections.Concurrent;
using System.Collections.Immutable;
using TicketShare.sdk;
using Toolbox.Extensions;

namespace TicketShareWeb.Components.Pages.Ticket.Model;

public sealed record TicketGroupModel
{
    public string TicketGroupId { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string ChannelId { get; init; } = null!;
    public ConcurrentDictionary<string, RoleModel> Roles { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public ConcurrentDictionary<string, SeatModel> Seats { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public bool Equals(TicketGroupModel? other)
    {
        return other is not null &&
            Name == other.Name &&
            Description == other.Description &&
            ChannelId == other.ChannelId &&
            Roles.DeepEquals(other.Roles) &&
            Seats.DeepEquals(other.Seats);
    }

    public override int GetHashCode() => HashCode.Combine(Name, Description, ChannelId, Roles, Seats);
}


public static class TicketGroupModelExtensions
{
    public static TicketGroupModel Clone(this TicketGroupModel subject) => new TicketGroupModel
    {
        TicketGroupId = subject.TicketGroupId,
        Name = subject.Name,
        Description = subject.Description,
        ChannelId = subject.ChannelId,
        Roles = subject.Roles.Values.Select(x => x.Clone().ToKeyValuePair(x.Id)).ToConcurrentDictionary(),
        Seats = subject.Seats.Values.Select(x => x.Clone().ToKeyValuePair(x.Id)).ToConcurrentDictionary(),
    };

    public static TicketGroupModel ConvertTo(this TicketGroupRecord subject) => new TicketGroupModel
    {
        TicketGroupId = subject.TicketGroupId,
        Name = subject.Name,
        Description = subject.Description,
        ChannelId = subject.ChannelId,
        Roles = subject.Roles.Select(x => x.ConvertTo().ToKeyValuePair(x.Id)).ToConcurrentDictionary(),
        Seats = subject.Seats.Select(x => x.ConvertTo().ToKeyValuePair(x.Id)).ToConcurrentDictionary(),
    };

    public static TicketGroupRecord ConvertTo(this TicketGroupModel subject) => new TicketGroupRecord
    {
        TicketGroupId = subject.TicketGroupId,
        Name = subject.Name,
        Description = subject.Description,
        ChannelId = subject.ChannelId,
        Roles = subject.Roles.Values.Select(x => x.ConvertTo()).ToImmutableArray(),
        Seats = subject.Seats.Values.Select(x => x.ConvertTo()).ToImmutableArray(),
    };
}
