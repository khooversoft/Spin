//using System.Collections.Immutable;
//using Microsoft.Extensions.Logging;
//using TicketApi.sdk.Model;
//using Toolbox.Extensions;
//using Toolbox.Rest;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace TicketApi.sdk;

//public readonly struct AttractionResult
//{
//    public IReadOnlyList<AttractionRecord> Attractions { get; init; }
//    public IReadOnlyList<ImageRecord> Images { get; init; }
//}

//public class TmAttractionClient
//{
//    protected readonly HttpClient _client;
//    private readonly ILogger<TicketEventClient> _logger;
//    private readonly TicketOption _ticketOption;
//    private readonly MonitorRate _monitorRate;

//    public TmAttractionClient(HttpClient client, TicketOption ticketOption, MonitorRate monitorRate, ILogger<TicketEventClient> logger)
//    {
//        _client = client.NotNull();
//        _ticketOption = ticketOption.NotNull();
//        _logger = logger.NotNull();
//        _monitorRate = monitorRate;
//    }

//    public async Task<Option<AttractionResult>> GetAttractions(IEnumerable<TeamDetail> teamDetails, ScopeContext context)
//    {
//        teamDetails.NotNull();

//        var searches = BuildSearch(teamDetails.ToArray());
//        var sequence = new Sequence<AttractionResult>();

//        foreach (var search in searches)
//        {
//            var attractionsOption = await GetAttractions(search, context);
//            if (attractionsOption.IsError()) return attractionsOption;

//            sequence += attractionsOption.Return();
//        }

//        var result = new AttractionResult
//        {
//            Attractions = sequence
//                .SelectMany(x => x.Attractions)
//                .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
//                .Select(x => x.First())
//                .ToImmutableArray(),

//            Images = sequence
//                .SelectMany(x => x.Images)
//                .Where(x => _ticketOption.IsImageSelected(x))
//                .GroupBy(x => x.Url, StringComparer.OrdinalIgnoreCase)
//                .Select(x => x.First())
//                .ToImmutableArray(),
//        };

//        return result;
//    }

//    public async Task<Option<AttractionResult>> GetAttractions(TicketMasterSearch search, ScopeContext context)
//    {
//        var sequenceOption = await GetAttractionsModels(search, context);
//        if (sequenceOption.IsError()) return sequenceOption.ToOptionStatus<AttractionResult>();
//        IReadOnlyList<AttractionModel> sequence = sequenceOption.Return();

//        var imageRecords = sequence
//            .SelectMany(x => x.Images, (o, i) => (id: o.Id, image: i))
//            .GroupBy(x => x.image.Url, StringComparer.OrdinalIgnoreCase)
//            .Select(x => x.First().Func(y => y.image.ConvertTo(y.id)));

//        var result = new AttractionResult
//        {
//            Attractions = sequence.Select(x => x.ConvertTo()).ToImmutableArray(),
//            Images = imageRecords.ToImmutableArray(),
//        };

//        return result;
//    }

//    public async Task<Option<IReadOnlyList<AttractionModel>>> GetAttractionsModels(TicketMasterSearch search, ScopeContext context)
//    {
//        var sequence = new Sequence<AttractionModel>();

//        while (true)
//        {
//            string query = search.Build();
//            string url = $"{_ticketOption.AttractionUrl}?{query}";

//            await _monitorRate.RecordEventAsync(context.CancellationToken);

//            var model = await new RestClient(_client)
//                .SetPath(url)
//                .GetAsync(context.With(_logger))
//                .GetContent<AttractionMasterModel>();

//            if (model.IsError()) return model.ToOptionStatus<IReadOnlyList<AttractionModel>>();
//            var ticketMasterModel = model.Return();
//            if (ticketMasterModel._embedded == null) break;

//            sequence += ticketMasterModel._embedded.Attractions;
//            search = search with { Page = search.Page + 1 };
//        }

//        return sequence.ToImmutableArray();
//    }

//    private IReadOnlyList<TicketMasterSearch> BuildSearch(IReadOnlyList<TeamDetail> teamDetails)
//    {
//        var list = teamDetails
//            //.Select(x => (segment: x.Segments.First().Id, genre: x.Genres.First().Id, subGenre: x.SubGenres.First().Id))
//            //.GroupBy(x => x)
//            .Select(x => new TicketMasterSearch(TicketSearchType.Attraction, _ticketOption, nameof(TmAttractionClient))
//            {
//                SegmentId = x.Segment.Id,
//                GenreId = x.Genre.Id,
//                SubGenreId = x.SubGenre.Id,
//                Size = 200,
//                Page = 0,
//            }).ToArray();

//        return list;
//    }
//}
