using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;


public sealed record EventRecord
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public DateTime? LocalDateTime { get; init; }
    public string? Timezone { get; init; }
    public string? SeatMapUrl { get; init; }
    public string VenueId { get; init; } = null!;
    public string AttractionIds { get; init; } = null!;

    public bool Equals(EventRecord? other) =>
        other != null &&
        Id == other.Id &&
        Name == other.Name &&
        Timezone == other.Timezone &&
        LocalDateTime == other.LocalDateTime &&
        SeatMapUrl == other.SeatMapUrl &&
        VenueId == other.VenueId &&
        VenueId == other.AttractionIds;

    public override int GetHashCode() => HashCode.Combine(Id, Name, LocalDateTime, Timezone, SeatMapUrl, VenueId, AttractionIds);

    public static IValidator<EventRecord> Validator { get; } = new Validator<EventRecord>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.LocalDateTime).ValidDateTimeOption()
        .RuleFor(x => x.VenueId).NotEmpty()
        .RuleFor(x => x.AttractionIds).NotEmpty()
        .Build();
}

public static class EventRecordExtensions
{
    public static Option Validate(this EventRecord record) => EventRecord.Validator.Validate(record).ToOptionStatus();
}