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

        Counters.AddHits();
        return classification.Cast<T>();
    }
}
