using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using TicketApi.sdk.TicketMasterAttraction;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;

public class TmAttractionClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<TmAttractionClient> _logger;
    private readonly TicketOption _ticketMasterOption;
    private const string _searchName = nameof(TmClassificationClient);

    public TmAttractionClient(HttpClient client, TicketOption ticketMasterOption, ILogger<TmAttractionClient> logger)
    {
        _client = client.NotNull();
        _ticketMasterOption = ticketMasterOption.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<AttractionCollectionRecord>> GetAttraction(TicketMasterSearch search, ScopeContext context)
    {
        var sequence = new Sequence<AttractionCollectionRecord>();
        int page = 0;
        const int pageSize = 200;

        while (true)
        {
            var query = new TicketMasterSearch(TicketSearchType.Attraction, _ticketMasterOption, _searchName)
            {
                Page = page,
                Size = pageSize,
                SegmentId = search.SegmentId.NotEmpty(),
                GenreId = search.GenreId.NotEmpty(),
                SubGenreId = search.SubGenreId.NotEmpty(),
            };

            string url = query.Build();

            var model = await new RestClient(_client)
                .SetPath(url)
                .GetAsync(context.With(_logger))
                .GetContent<AttractionRootModel>();

            if (model.IsError()) return model.ToOptionStatus<AttractionCollectionRecord>();
            var masterModel = model.Return();
            if (masterModel._embedded == null) break;

            sequence += masterModel.ConvertTo();
            page++;

            if (masterModel.page.totalElements < query.Size) break;
        }

        var result = new AttractionCollectionRecord()
        {
            Attractions = sequence.SelectMany(x => x.Attractions).ToImmutableArray(),
        };

        return result;
    }
}