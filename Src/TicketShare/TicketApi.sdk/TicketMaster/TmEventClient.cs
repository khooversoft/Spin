using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;

public class TmEventClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<TmEventClient> _logger;
    private readonly TicketOption _ticketMasterOption;
    private const string _searchName = nameof(TmEventClient);

    public TmEventClient(HttpClient client, TicketOption ticketMasterOption, ILogger<TmEventClient> logger)
    {
        _client = client.NotNull();
        _ticketMasterOption = ticketMasterOption.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<EventCollectionRecord>> GetEvents(TicketMasterSearch search, ScopeContext context)
    {
        search.NotNull();

        const int pageSize = 200;
        var sequence = new Sequence<EventCollectionRecord>();
        int page = 0;

        while (true)
        {
            var query = new TicketMasterSearch(TicketSearchType.Event, _ticketMasterOption, _searchName)
            {
                Page = page,
                Size = pageSize,
                AttractionId = search.AttractionId.NotEmpty(),
            };

            string url = query.Build();

            var model = await new RestClient(_client)
                .SetPath(url)
                .GetAsync(context.With(_logger))
                .GetContent<TicketMasterEvent.EventRootModel>();

            if (model.IsError()) return model.ToOptionStatus<EventCollectionRecord>();
            var masterModel = model.Return();
            if (masterModel._embedded == null) break;

            sequence += masterModel.ConvertTo();
            page++;

            if (masterModel.page.totalElements <= sequence.SelectMany(x => x.Events).Count()) break;
        }

        var result = new EventCollectionRecord()
        {
            Events = sequence.SelectMany(x => x.Events).ToImmutableArray(),
        };

        return result;
    }
}
