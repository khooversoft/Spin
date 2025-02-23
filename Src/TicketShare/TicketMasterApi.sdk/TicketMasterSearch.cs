using Toolbox.Extensions;

namespace TicketMasterApi.sdk;

public readonly struct TicketMasterSearch
{
    public string Keywords { get; init; }
    public DateTime? StartDateTime { get; init; }
    public DateTime? EndDateTime { get; init; }
    public string? PromoterId { get; init; }
    public int? Page { get; init; }
    public int? Size { get; init; }

    public string GetQuery(string apiKey)
    {
        var query = new[]
        {
            $"apikey={apiKey}",
            Keywords.ToNullIfEmpty() != null ? $"keyword={Uri.EscapeDataString(Keywords)}" : null,
            "locale=*",
            StartDateTime?.Func(x => $"startDateTime={dateTimeFormat(x)}"),
            EndDateTime?.Func(x => $"endDateTime={dateTimeFormat(x)}"),
            PromoterId?.Func(x => $"promoterId={x}"),
            Page?.Func(x => $"page={x}"),
            Size?.Func(x => $"size={x}"),
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
}
