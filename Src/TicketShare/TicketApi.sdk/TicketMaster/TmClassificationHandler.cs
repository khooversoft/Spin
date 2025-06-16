using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;

public class TmClassificationHandler : DataProviderBase
{
    private const string _prefixPath = "config/TicketData";
    private readonly ILogger<TmClassificationHandler> _logger;
    private readonly TmClassificationClient _classificationClient;
    private readonly TicketOption _ticketOption;

    public TmClassificationHandler(TmClassificationClient classificationClient, TicketOption ticketOption, ILogger<TmClassificationHandler> logger)
    {
        _classificationClient = classificationClient;
        _ticketOption = ticketOption;
        _logger = logger.NotNull();
    }

    public override Task<Option<string>> Exists(string key, ScopeContext context) => new Option<string>(StatusCode.OK).ToTaskResult();

    public override async Task<Option<T>> Get<T>(string key, object? state, ScopeContext context)
    {
        var classificationOption = await _classificationClient.GetClassifications(context);
        if (classificationOption.IsError()) return classificationOption.ToOptionStatus<T>();

        ClassificationRecord classification = classificationOption.Return();

        var result = new ClassificationRecord
        {
            Segments = classification.Segments.Where(x => _ticketOption.ShouldInclude(x.Name))
                .Select(c => c with
                {
                    Genres = c.Genres.Where(g => shouldInclude(c.Name, g.Name))
                    .Select(g => g with
                    {
                        SubGenres = g.SubGenres.Where(sg => shouldIncludeSub(c.Name, g.Name, sg.Name)).ToImmutableArray()
                    }).ToImmutableArray()
                })
                .ToImmutableArray()
        };


        Counters.AddHits();
        return result.Cast<T>();

        bool shouldInclude(string segmentName, string genreName)
        {
            string path = $"{segmentName}/{genreName}";
            return _ticketOption.ShouldInclude(path);
        }

        bool shouldIncludeSub(string segmentName, string genreName, string subGenreName)
        {
            string path = $"{segmentName}/{genreName}/{subGenreName}";
            return _ticketOption.ShouldInclude(path);
        }
    }
}
