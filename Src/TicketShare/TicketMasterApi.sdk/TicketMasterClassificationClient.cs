using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TicketMasterApi.sdk.Model.Classification;
using TicketMasterApi.sdk.Model.Event;
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
                "size=1",
            }
            .Where(x => x.IsNotEmpty())
            .Join('&');

            string url = $"{_ticketMasterOption.ClassificationUrl}?{query}";

            var model = await new RestClient(_client)
                .SetPath(url)
                .GetAsync(context.With(_logger));

            string json = model.Content;
            var model2 = json?.ToObject<ClassificationMasterModel>();
                //.GetContent<ClassificationMasterModel>();

            //if (model.IsError()) return model.ToOptionStatus<IReadOnlyList<ClassificationRecord>>();
            //var masterModel = model.Return();
            //if (masterModel._embedded == null) break;

            //sequence += masterModel._embedded.Classifications;
            break;
        }

        //var result = sequence.Select(x => ConvertToRecord(x)).ToImmutableArray();
        //return result;
        return default;
    }

    //private ClassificationRecord ConvertToRecord(ClassificationModel subject)
    //{
    //    subject.NotNull();

    //    var segment = subject.Segments.FirstOrDefault();
    //    var grene = segment?._embedded?.genres.FirstOrDefault();
    //    var subGrene = grene?._embedded?.subgenres.FirstOrDefault();

    //    var segmentData = segment?.Func(x => (x.Id, x.Name));
    //    var greneData = grene?.Func(x => (x.Id, x.Name));
    //    var subGreneData = subGrene?.Func(x => (x.Id, x.Name));

    //    var result = new ClassificationRecord
    //    {
    //        SegmentId = segmentData?.Id,
    //        Segment = segmentData?.Name,
    //        GenreId = greneData?.Id,
    //        Genre = greneData?.Name,
    //        SubGenreId = greneData?.Id,
    //        SubGenre = greneData?.Name,
    //    };

    //    return result;
    //}
}
