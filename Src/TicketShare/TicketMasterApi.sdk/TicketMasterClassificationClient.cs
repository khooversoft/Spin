using System.Collections.Immutable;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TicketMasterApi.sdk.Model.Classification;
using Toolbox.Extensions;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketMasterApi.sdk;

public class TicketMasterClassificationClient
{
    private const string _cacheKey = nameof(TicketMasterClassificationClient);
    protected readonly HttpClient _client;
    private readonly ILogger<TicketMasterEventClient> _logger;
    private readonly TicketMasterOption _ticketMasterOption;
    private readonly IMemoryCache _memoryCache;

    private readonly MemoryCacheEntryOptions _memoryOptions = new MemoryCacheEntryOptions
    {
        SlidingExpiration = TimeSpan.FromMinutes(30)
    };

    public TicketMasterClassificationClient(HttpClient client, TicketMasterOption ticketMasterOption, IMemoryCache memoryCache, ILogger<TicketMasterEventClient> logger)
    {
        _client = client.NotNull();
        _ticketMasterOption = ticketMasterOption.NotNull();
        _memoryCache = memoryCache.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<IReadOnlyList<ClassificationRecord>>> GetClassifications(ScopeContext context)
    {
        if (_ticketMasterOption.UseCache)
        {
            if (_memoryCache.TryGetValue<IReadOnlyList<ClassificationRecord>>(_cacheKey, out var data))
            {
                return data.NotNull().ToOption();
            }
        }

        var result = await InternalGet(context);
        if (result.IsError()) return result;

        if (_ticketMasterOption.UseCache)
        {
            var resultData = result.Return();
            _memoryCache.Set(_cacheKey, resultData, _memoryOptions);
        }

        return result;
    }


    public async Task<Option<IReadOnlyList<ClassificationRecord>>> InternalGet(ScopeContext context)
    {
        var sequence = new Sequence<ClassificationModel>();
        int page = 0;

        while (true)
        {
            var query = new[]
            {
                $"apikey={_ticketMasterOption.ApiKey}",
                $"page={page}",
                "size=1000",
            }
            .Where(x => x.IsNotEmpty())
            .Join('&');

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
