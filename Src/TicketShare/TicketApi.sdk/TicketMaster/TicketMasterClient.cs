using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;

public class TicketMasterClient
{
    public const string _prefixPath = "config/TicketData";
    private readonly ILogger<TicketMasterClient> _logger;
    private readonly IDataClient<ClassificationRecord> _classificationClient;
    private readonly IDataClient<EventCollectionRecord> _eventClient;
    private readonly TicketOption _ticketOption;
    private readonly IDataClient<AttractionCollectionRecord> _attractionClient;

    public TicketMasterClient(
        IDataClient<ClassificationRecord> classificationClient,
        IDataClient<EventCollectionRecord> eventClient,
        IDataClient<AttractionCollectionRecord> attractionClient,
        TicketOption ticketOption,
        ILogger<TicketMasterClient> logger)
    {
        _classificationClient = classificationClient.NotNull();
        _eventClient = eventClient.NotNull();
        _attractionClient = attractionClient;
        _ticketOption = ticketOption.NotNull();
        _logger = logger.NotNull();
    }

    private static string CreatePath(string searchName) => $"{_prefixPath}/{searchName.NotEmpty()}.json";

    public async Task<Option<ClassificationRecord>> GetClassifications(ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Getting classification data from TicketMasterClient");

        var search = new TicketMasterSearch(TicketSearchType.Classification, _ticketOption, "classification");
        string path = CreatePath(search.SearchName);
        var option = await _classificationClient.Get(path, search, context);
        return option;
    }

    public async Task<Option<EventCollectionRecord>> GetEvents(SegmentRecord segmentRecord, GenreRecord genreRecord, SubGenreRecord subGenreRecord, ScopeContext context)
    {
        segmentRecord.NotNull();
        genreRecord.NotNull();
        subGenreRecord.NotNull();
        context = context.With(_logger);

        string segmentName = $"{PathTool.ToValidFileName(segmentRecord.Name)}-{segmentRecord.Id}";
        string genreName = $"{PathTool.ToValidFileName(genreRecord.Name)}-{genreRecord.Id}";
        string subGenreName = $"{PathTool.ToValidFileName(subGenreRecord.Name)}-{subGenreRecord.Id}";

        string searchName = $"events-{segmentName}-{genreName}-{subGenreName}";
        context.LogDebug("Getting events data from TicketMasterClient, searchName={searchName}", searchName);
        var search = new TicketMasterSearch(TicketSearchType.Event, _ticketOption, searchName)
        {
            SegmentId = segmentRecord.Id.NotEmpty(),
            GenreId = genreRecord.Id.NotEmpty(),
            SubGenreId = subGenreRecord.Id.NotEmpty(),
        };

        string path = CreatePath(search.SearchName);
        var option = await _eventClient.Get(path, search, context);
        return option;
    }

    public async Task<Option<AttractionCollectionRecord>> GetAttractions(SegmentRecord segmentRecord, GenreRecord genreRecord, SubGenreRecord subGenreRecord, ScopeContext context)
    {
        segmentRecord.NotNull();
        genreRecord.NotNull();
        subGenreRecord.NotNull();
        context = context.With(_logger);

        string segmentName = $"{PathTool.ToValidFileName(segmentRecord.Name)}-{segmentRecord.Id}";
        string genreName = $"{PathTool.ToValidFileName(genreRecord.Name)}-{genreRecord.Id}";
        string subGenreName = $"{PathTool.ToValidFileName(subGenreRecord.Name)}-{subGenreRecord.Id}";

        string searchName = $"attractions-{segmentName}-{genreName}-{subGenreName}";
        context.LogDebug("Getting attractions data from TicketMasterClient, searchName={searchName}", searchName);

        var search = new TicketMasterSearch(TicketSearchType.Attraction, _ticketOption, searchName)
        {
            SegmentId = segmentRecord.Id.NotEmpty(),
            GenreId = genreRecord.Id.NotEmpty(),
            SubGenreId = subGenreRecord.Id.NotEmpty(),
        };

        string path = CreatePath(search.SearchName);
        var option = await _attractionClient.Get(path, search, context);
        return option;
    }
}
