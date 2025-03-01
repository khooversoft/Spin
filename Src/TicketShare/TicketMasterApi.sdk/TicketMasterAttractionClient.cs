using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using TicketMasterApi.sdk.MasterList;
using TicketMasterApi.sdk.Model.Attraction;
using Toolbox.Extensions;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketMasterApi.sdk;

public class TicketMasterAttractionClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<TicketMasterEventClient> _logger;
    private readonly TicketMasterOption _ticketMasterOption;

    public TicketMasterAttractionClient(HttpClient client, TicketMasterOption ticketMasterOption, ILogger<TicketMasterEventClient> logger)
    {
        _client = client.NotNull();
        _ticketMasterOption = ticketMasterOption.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<IReadOnlyList<AttractionRecord>>> GetAttractions(IReadOnlyList<TeamDetail> teamDetails, ScopeContext context)
    {
        teamDetails.NotNull();
        var searches = BuildSearch(teamDetails);

        var sequence = new Sequence<AttractionRecord>();

        foreach(var search in searches)
        {
            var result = await InternalGet(search, context);
            if (result.IsError()) return result;

            sequence += result.Return();
        }

        return default;
    }

    private IReadOnlyList<TicketMasterSearch> BuildSearch(IReadOnlyList<TeamDetail> teamDetails)
    {
        var list = teamDetails
            .Select(x => (segment: x.Segments.First().Id, genre: x.Genres.First().Id, subGenre: x.SubGenres.First().Id))
            .GroupBy(x => x)
            .Select(x => new TicketMasterSearch
            {
                SegmentId = x.Key.segment,
                GenreId = x.Key.genre,
                SubGenreId = x.Key.subGenre,
                Size = 200,
                Page = 0,
            }).ToArray();

        return list;
    }

    public async Task<Option<IReadOnlyList<AttractionRecord>>> InternalGet(TicketMasterSearch search, ScopeContext context)
    {
        var sequence = new Sequence<AttractionModel>();

        while (true)
        {
            string query = search.GetQuery(_ticketMasterOption.ApiKey);
            string url = $"{_ticketMasterOption.AttriactionUrl}?{query}";

            var model = await new RestClient(_client)
                .SetPath(url)
                .GetAsync(context.With(_logger))
                .GetContent<AttractionMasterModel>();

            if (model.IsError()) return model.ToOptionStatus<IReadOnlyList<AttractionRecord>>();
            var ticketMasterModel = model.Return();
            if (ticketMasterModel._embedded == null) break;

            sequence += ticketMasterModel._embedded.Attractions;
            search = search with { Page = search.Page + 1 };
        }

        var result = sequence.Select(x => x.ConvertTo()).ToImmutableArray();
        return result;
    }
}
