using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TicketApi.sdk.TicketMasterClassification;
using Toolbox.Extensions;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;

public class TmClassificationClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<TmEventClient> _logger;
    private readonly TicketOption _ticketMasterOption;
    private const string _searchName = nameof(TmClassificationClient);

    public TmClassificationClient(HttpClient client, TicketOption ticketMasterOption, ILogger<TmEventClient> logger)
    {
        _client = client.NotNull();
        _ticketMasterOption = ticketMasterOption.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<ClassificationRecord>> GetClassifications(ScopeContext context)
    {
        var sequence = new Sequence<ClassificationRecord>();
        int page = 0;

        while (true)
        {
            var query = new TicketMasterSearch(TicketSearchType.Classification, _ticketMasterOption, _searchName)
            {
                Page = page,
                Size = 1000,
            };

            string url = query.Build();

            var model = await new RestClient(_client)
                .SetPath(url)
                .GetAsync(context.With(_logger))
                .GetContent<ClassificationRootModel>();

            if (model.IsError()) return model.ToOptionStatus<ClassificationRecord>();
            var masterModel = model.Return();
            if (masterModel._embedded == null) break;

            sequence += masterModel.ConvertTo();
            page++;
            if (page > 3) Debugger.Break();

            if (masterModel.page.totalElements < query.Size) break;
        }

        var result = new ClassificationRecord()
        {
            Segements = sequence.SelectMany(x => x.Segements).ToImmutableArray(),
        };

        return result;
    }
}
