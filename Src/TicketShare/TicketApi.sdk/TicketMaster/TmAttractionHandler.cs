using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;

public class TmAttractionHandler : DataProviderBase
{
    public const string _prefixPath = "config/TicketData";
    private readonly ILogger<TmEventHandler> _logger;
    private readonly TmAttractionClient _attractionClient;
    private readonly TicketOption _ticketOption;

    public TmAttractionHandler(TmAttractionClient attractionClient, TicketOption ticketOption, ILogger<TmEventHandler> logger)
    {
        _attractionClient = attractionClient;
        _ticketOption = ticketOption;
        _logger = logger.NotNull();
    }

    public override Task<Option<string>> Exists(string key, ScopeContext context) => new Option<string>(StatusCode.OK).ToTaskResult();

    public override async Task<Option<T>> Get<T>(string key, object? state, ScopeContext context)
    {
        TicketMasterSearch search = state as TicketMasterSearch ?? throw new ArgumentException("State must be of type TicketMasterSearch", nameof(state));

        var readOption = await _attractionClient.GetAttraction(search, context);
        if (readOption.IsError()) return readOption.ToOptionStatus<T>();

        var attractionCollection = readOption.Return();

        var result = new AttractionCollectionRecord
        {
            Attractions = attractionCollection.Attractions
                .Where(x => !x.Name.Contains("test", StringComparison.OrdinalIgnoreCase))
                .Where(x => x.Classifications.Any(y => y.SubType?.Name?.EqualsIgnoreCase("Team") == true))
                .ToImmutableArray()
        };

        return result.Cast<T>();
    }
}
