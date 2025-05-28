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
    private readonly TicketClassificationClient _classificationClient;
    private readonly SearchValues<string> _searchValues = SearchValues.Create(["Sports", "Music"], StringComparison.OrdinalIgnoreCase);

    public TicketMasterClient(TicketClassificationClient classificationClient, ILogger<TicketMasterClient> logger)
    {
        _classificationClient = classificationClient.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<T>> Get<T>(TicketMasterSearch search, ScopeContext context)
    {
        context = context.With(_logger);
        context.LogDebug("Getting data from ticket master for searchName={searchName}", search.SearchName);

        if (search.SearchType == TicketSearchType.Classification)
        {
            var classificationOption = await _classificationClient.GetClassifications(context);
            if (classificationOption.IsError()) return classificationOption.ToOptionStatus<T>();

            ClassificationRecord classification = classificationOption.Return();

            var result = new ClassificationRecord
            {
                Segements = classification.Segements.Where(x => _searchValues.Contains(x.Name)).ToImmutableArray(),
            };

            return result.Cast<T>();
        }

        throw new System.Diagnostics.UnreachableException();
    }
}
