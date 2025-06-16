using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;

public enum TicketSearchType
{
    Event,
    Attraction,
    Classification,
    //Venue,
    //Segment,
    //Genre,
    //SubGenre,
}

public record TicketMasterSearch
{
    public TicketMasterSearch(TicketSearchType searchType, TicketOption ticketOption, string searchName)
    {
        searchType.IsEnumValid().BeTrue();
        ticketOption.Validate().ThrowOnError();
        searchName.NotEmpty();

        SearchType = searchType;
        TicketOption = ticketOption;
        SearchName = searchName;
    }

    public TicketSearchType SearchType { get; }
    public TicketOption TicketOption { get; }
    public string SearchName { get; }
    public string? Keywords { get; init; }
    public DateTime? StartDateTime { get; init; }
    public DateTime? EndDateTime { get; init; }
    public string? City { get; init; }
    public string? PromoterId { get; init; }
    public int? Page { get; init; }
    public int? Size { get; init; }
    public string? AttractionId { get; init; }
    public string? SegmentId { get; init; }
    public string? GenreId { get; init; }
    public string? SubGenreId { get; init; }

    public string Build()
    {
        string baseUri = SearchType switch
        {
            TicketSearchType.Event => TicketOption.EventUrl,
            TicketSearchType.Attraction => TicketOption.AttractionUrl,
            TicketSearchType.Classification => TicketOption.ClassificationUrl,
            _ => throw new ArgumentOutOfRangeException(nameof(SearchType), SearchType, null),
        };

        int size = Size ?? TicketOption.BatchSize;

        var query = new[]
        {
            $"apikey={TicketOption.ApiKey}",
            Keywords.ToNullIfEmpty() != null ? $"keyword={Uri.EscapeDataString(Keywords.NotEmpty())}" : null,
            "locale=*",
            StartDateTime?.Func(x => $"startDateTime={dateTimeFormat(x)}"),
            EndDateTime?.Func(x => $"endDateTime={dateTimeFormat(x)}"),
            City?.Func(x => $"city={x}"),
            PromoterId?.Func(x => $"promoterId={x}"),
            Page?.Func(x => $"page={x}"),
            Size?.Func(x => $"size={x}"),
            AttractionId?.Func(x => $"attractionId={x}"),
            SegmentId?.Func(x => $"segmentId={x}"),
            GenreId?.Func(x => $"genreId={x}"),
            SubGenreId?.Func(x => $"subGenreId={x}"),
        }
        .Where(x => x.IsNotEmpty())
        .Join('&');

        return baseUri + "?" + query;

        string? dateTimeFormat(DateTime? subject) => subject switch
        {
            null => throw new ArgumentException("DateTime is required"),
            var v => v.Value.ToString("yyyy-MM-ddTHH:mm:ssZ"),
        };
    }

    public static IValidator<TicketMasterSearch> Validator => new Validator<TicketMasterSearch>()
        .RuleFor(x => x.Keywords).Must(x => true, _ => "")
        .Build();
}
