using Toolbox.Extensions;
using Toolbox.Tools;

namespace TicketApi.sdk;

public record TicketMasterSearch
{
    public string ApiKey { get; set; } = null!;
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

    public string Build(string apiKey)
    {
        ApiKey = apiKey;
        return Build();
    }

    public string Build()
    {
        ApiKey.NotEmpty();

        var query = new[]
        {
            $"apikey={ApiKey}",
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

        return query;

        string? dateTimeFormat(DateTime? subject) => subject switch
        {
            null => throw new ArgumentException("DateTime is required"),
            var v => v.Value.ToString("yyyy-MM-ddTHH:mm:ssZ"),
        };
    }

    public string GetQueryHash() => Build("query").ToHashHex();

    public static IValidator<TicketMasterSearch> Validator => new Validator<TicketMasterSearch>()
        .RuleFor(x => x.Keywords).Must(x => true, _ => "")
        .Build();
}
