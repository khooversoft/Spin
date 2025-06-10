using System.Buffers;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;

public class TicketMasterClient
{
    private readonly ILogger<TicketMasterClient> _logger;
    private readonly TmClassificationClient _classificationClient;
    private readonly SearchValues<string> _classificationFilter = SearchValues.Create(["Sports", "Music"], StringComparison.OrdinalIgnoreCase);
    private readonly TicketEventClient _eventClient;

    public TicketMasterClient(TmClassificationClient classificationClient, TicketEventClient eventClient, ILogger<TicketMasterClient> logger)
    {
        _classificationClient = classificationClient.NotNull();
        _eventClient = eventClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<T>> Get<T>(TicketMasterSearch search, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Getting data from ticket master for searchName={searchName}", search.SearchName);

        switch (search.SearchType)
        {
            case TicketSearchType.Classification:
                var classificationOption = await _classificationClient.GetClassifications(context);
                if (classificationOption.IsError()) return classificationOption.ToOptionStatus<T>();

                ClassificationRecord classification = classificationOption.Return();

                var result = new ClassificationRecord
                {
                    Segements = classification.Segements.Where(x => _classificationFilter.Contains(x.Name)).ToImmutableArray(),
                };

                return result.Cast<T>();

            case TicketSearchType.Event:
                var eventOption = await _eventClient.GetEvents(search, context);
                if (eventOption.IsError()) return eventOption.ToOptionStatus<T>();

                return eventOption.Return().Cast<T>();

            default:
                throw new System.Diagnostics.UnreachableException();
        }
    }
}
