using System.Collections.Frozen;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using TicketApi.sdk.MasterList;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;

public class TicketDataManager
{
    private readonly ILogger<TicketDataManager> _logger;
    private readonly TicketDataClient _dataClient;
    private readonly TicketAttractionClient _attractionClient;
    private readonly TicketEventClient _eventClient;
    private static readonly FrozenDictionary<string, TeamDetail> _teamDetails = TeamMasterList.GetDetails().ToFrozenDictionary(x => x.Name);

    public TicketDataManager(TicketDataClient dataClient, TicketAttractionClient attractionClient, TicketEventClient eventClient, ILogger<TicketDataManager> logger)
    {
        _dataClient = dataClient.NotNull();
        _attractionClient = attractionClient.NotNull();
        _eventClient = eventClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<TicketDataRecord>> Get(ScopeContext context)
    {
        context = context.With(_logger);

        var result = await _dataClient.Get(context);
        result.LogStatus(context, "Get data");
        return result;
    }

    public async Task<Option> Build(ScopeContext context)
    {
        context = context.With(_logger);

        var ticketData = (await _dataClient.Get(context)) switch
        {
            { StatusCode: StatusCode.NotFound } => new TicketDataRecord(),
            var v => v,
        };

        if (ticketData.IsError()) return ticketData.LogStatus(context, "Get ticket data").ToOptionStatus();

        ticketData = await Attractions(ticketData.Return(), context);
        if (ticketData.IsError()) return ticketData.ToOptionStatus();

        ticketData = await GetEvents(ticketData.Return(), context);
        if (ticketData.IsError()) return ticketData.ToOptionStatus();

        return StatusCode.OK;
    }

    public async Task<Option> ClearData(ScopeContext context)
    {
        context = context.With(_logger);

        var clearData = await _dataClient.CleatData(context);
        clearData.LogStatus(context, "Clear data");
        if (clearData.IsError()) return clearData;

        context.LogTrace("Ticked master data cleared");
        return clearData;
    }

    private async Task<Option<TicketDataRecord>> Attractions(TicketDataRecord ticketData, ScopeContext context)
    {
        context = context.With(_logger);
        if (ticketData.Attractions.Count > 0) return ticketData;

        var resultOption = await _attractionClient.GetAttractions(_teamDetails.Values, context);
        if (resultOption.IsError()) return resultOption.LogStatus(context, "Get attractions").ToOptionStatus<TicketDataRecord>();
        AttractionResult attractionResult = resultOption.Return();

        var dict = ticketData.Attractions.ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase);

        var list = attractionResult.Attractions.Where(x => _teamDetails.ContainsKey(x.Name)).ToArray();
        list.ForEach(x => dict[x.Id] = x);

        var result = ticketData with
        {
            Attractions = dict.Values.ToArray(),
            Images = attractionResult.Images.Concat(ticketData.Images)
                .GroupBy(x => x.Url, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First())
                .ToImmutableArray(),
        };

        var updateResult = await UpdateData(result, context);
        if (updateResult.IsError()) return updateResult.ToOptionStatus<TicketDataRecord>();

        return result;
    }

    private async Task<Option<TicketDataRecord>> GetEvents(TicketDataRecord ticketData, ScopeContext context)
    {
        context = context.With(_logger);
        if (ticketData.Events.Count > 0) return ticketData;

        var attractionIds = ticketData.Attractions
            .Select(x => x.Id)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var eventResultOption = await _eventClient.GetEvents(attractionIds, context);
        if (eventResultOption.IsError()) return eventResultOption.LogStatus(context, "Get events").ToOptionStatus<TicketDataRecord>();
        var eventResult = eventResultOption.Return();

        var eventDict = ticketData.Events
            .ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase)
            .Action(x => eventResult.Events.ForEach(y => x[y.Id] = y))
            .Select(x => x.Value);

        var venueDict = ticketData.Venues
            .ToDictionary(x => x.Id, StringComparer.OrdinalIgnoreCase)
            .Action(x => eventResult.Venues.ForEach(y => x[y.Id] = y))
            .Select(x => x.Value);

        var imageDict = ticketData.Images
            .ToDictionary(x => x.Url, StringComparer.OrdinalIgnoreCase)
            .Action(x => eventResult.Images.ForEach(y => x[y.Url] = y))
            .Select(x => x.Value);

        var result = ticketData with
        {
            Events = eventDict.ToImmutableArray(),
            Venues = venueDict.ToImmutableArray(),
            Images = imageDict.ToImmutableArray(),
        };

        var updateResult = await UpdateData(result, context);
        if (updateResult.IsError()) return updateResult.ToOptionStatus<TicketDataRecord>();

        return result;
    }

    private async Task<Option> UpdateData(TicketDataRecord ticketData, ScopeContext context)
    {
        var result = await _dataClient.Set(ticketData, context);
        result.LogStatus(context, "Update data");
        return result.ToOptionStatus();
    }
}
