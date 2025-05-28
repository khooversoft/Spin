//using System.Collections.Immutable;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace TicketApi.sdk;

///// <summary>
///// Questions asked of the data
/////     1) What leagues are available?
/////     2) What teams for reach league
/////     3) What attractions for each team
/////     4) Get venu information
///// </summary>
//public sealed record TicketDataRecord
//{
//    public IReadOnlyList<AttractionRecord> Attractions { get; init; } = Array.Empty<AttractionRecord>();
//    public IReadOnlyList<EventRecord> Events { get; init; } = Array.Empty<EventRecord>();
//    public IReadOnlyList<VenueRecord> Venues { get; init; } = Array.Empty<VenueRecord>();
//    public IReadOnlyList<ImageRecord> Images { get; init; } = Array.Empty<ImageRecord>();
//    public DateTime LastUpdated { get; init; } = DateTime.UtcNow;
//    public DateTime? EventLastUpdated { get; init; }

//    public bool Equals(TicketDataRecord? other)
//    {
//        var result = other is TicketDataRecord subject &&
//            Attractions.OrderBy(x => x.Id).SequenceEqual(subject.Attractions) &&
//            Events.OrderBy(x => x.Id).SequenceEqual(subject.Events.OrderBy(x => x.Id)) &&
//            Venues.OrderBy(x => x.Id).SequenceEqual(subject.Venues.OrderBy(x => x.Id)) &&
//            Images.OrderBy(x => x.Url).SequenceEqual(subject.Images.OrderBy(x => x.Url)) &&
//            LastUpdated == subject.LastUpdated &&
//            EventLastUpdated == subject.EventLastUpdated;

//        return result;
//    }

//    public override int GetHashCode() => HashCode.Combine(Attractions, Events, Venues);

//    public static IValidator<TicketDataRecord> Validator { get; } = new Validator<TicketDataRecord>()
//        .RuleForEach(x => x.Attractions).Validate(AttractionRecord.Validator)
//        .RuleForEach(x => x.Events).Validate(EventRecord.Validator)
//        .RuleForEach(x => x.Venues).Validate(VenueRecord.Validator)
//        .RuleForEach(x => x.Images).Validate(ImageRecord.Validator)
//        .RuleFor(x => x.LastUpdated).ValidDateTime()
//        .RuleFor(x => x.EventLastUpdated).ValidDateTimeOption()
//        .Build();
//}


//public static class TicketDataModelTool
//{
//    public static Option Validate(this TicketDataRecord subject) => TicketDataRecord.Validator.Validate(subject).ToOptionStatus();

//    public static IReadOnlyList<(string League, AttractionRecord Team)> GetLeagues(this TicketDataRecord subject)
//    {
//        subject.NotNull();

//        var list = TeamMasterList.GetDetails()
//           .Join(subject.Attractions, x => x.Name, x => x.Name, (t, a) => (league: t.League, team: a), StringComparer.OrdinalIgnoreCase)
//           .ToImmutableArray();

//        return list;
//    }

//    public static IReadOnlyList<EventRecord> GetEvents(this TicketDataRecord subject, string attractionId)
//    {
//        subject.NotNull();
//        attractionId.NotEmpty();

//        var list = subject.Events
//            .Where(x => x.AttractionIds.Split(',').Contains(attractionId, StringComparer.OrdinalIgnoreCase))
//            .ToImmutableArray();

//        return list;
//    }

//    public static IReadOnlyList<VenueRecord> GetVenue(this TicketDataRecord subject, string venueId)
//    {
//        subject.NotNull();
//        venueId.NotEmpty();

//        var list = subject.Venues
//            .Where(x => x.Id == venueId)
//            .ToImmutableArray();

//        return list;
//    }
//}