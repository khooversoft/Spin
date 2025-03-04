using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using TicketApi.sdk.Model;
using Toolbox.Extensions;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;

public class TicketClassificationClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<TicketEventClient> _logger;
    private readonly TicketOption _ticketMasterOption;

    public TicketClassificationClient(HttpClient client, TicketOption ticketMasterOption, ILogger<TicketEventClient> logger)
    {
        _client = client.NotNull();
        _ticketMasterOption = ticketMasterOption.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<IReadOnlyList<ClassificationRecord>>> GetClassifications(ScopeContext context)
    {
        var sequence = new Sequence<ClassificationModel>();
        int page = 0;

        while (true)
        {
            var query = new TicketMasterSearch
            {
                ApiKey = _ticketMasterOption.ApiKey,
                Page = page,
                Size = 1000,
            };

            string url = $"{_ticketMasterOption.ClassificationUrl}?{query}";

            var model = await new RestClient(_client)
                .SetPath(url)
                .GetAsync(context.With(_logger))
                .GetContent<ClassificationMasterModel>();

            if (model.IsError()) return model.ToOptionStatus<IReadOnlyList<ClassificationRecord>>();
            var masterModel = model.Return();
            if (masterModel._embedded == null) break;

            sequence += masterModel._embedded.Classifications;
            page++;
        }

        var result = sequence
            .SelectMany(ConvertToRecord)
            .ToImmutableArray();

        return result;
    }

    private IReadOnlyList<ClassificationRecord> ConvertToRecord(ClassificationModel subject)
    {
        subject.NotNull();
        if (subject.Segment == null) return Array.Empty<ClassificationRecord>();

        (Class_SegmentModel seg, Class_GenreModel grene, Class_SubGenreModel subgrene)[] list = subject.NotNull().Segment.NotNull().ToEnumerable()
            .Select(x => (seg: x, grene: x?._embedded?.genres))
            .SelectMany(x => x.grene ?? Array.Empty<Class_GenreModel>(), (o, i) => (o.seg, grene: i))
            .SelectMany(x => x.grene._embedded?.subgenres ?? Array.Empty<Class_SubGenreModel>(), (o, i) => (o.seg, o.grene, subgrene: i))
            .ToArray();

        var result = list
            .Select(x =>
            {
                return new ClassificationRecord
                {
                    Segement = x.seg.ConvertTo(),
                    Grene = x.grene.ConvertTo(),
                    SubGrene = x.subgrene.ConvertTo(),
                };
            })
            .ToArray();

        return result;
    }
}
