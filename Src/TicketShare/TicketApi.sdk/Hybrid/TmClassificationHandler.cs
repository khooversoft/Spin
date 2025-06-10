using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketApi.sdk;

public class TmClassificationHandler : IDataProvider
{
    private readonly SearchValues<string> _classificationFilter = SearchValues.Create(["Sports", "Music"], StringComparison.OrdinalIgnoreCase);
    private readonly ILogger<TmClassificationHandler> _logger;
    private readonly TmClassificationClient _classificationClient;
    private readonly TicketOption _ticketOption;

    public TmClassificationHandler(TmClassificationClient classificationClient, TicketOption ticketOption, ILogger<TmClassificationHandler> logger)
    {
        _classificationClient = classificationClient;
        _ticketOption = ticketOption;
        _logger = logger.NotNull();
    }

    public string Name => throw new NotImplementedException();

    public DataClientCounters Counters { get; } = new DataClientCounters();

    public Task<Option> Delete(string key, ScopeContext context) => new Option(StatusCode.OK).ToTaskResult();
    public Task<Option<string>> Exists(string key, ScopeContext context) => new Option<string>(StatusCode.NotFound).ToTaskResult();

    public async Task<Option<T>> Get<T>(string key, ScopeContext context)
    {
        var classificationOption = await _classificationClient.GetClassifications(context);
        if (classificationOption.IsError()) return classificationOption.ToOptionStatus<T>();

        ClassificationRecord classification = classificationOption.Return();

        var result = new ClassificationRecord
        {
            Segements = classification.Segements.Where(x => _classificationFilter.Contains(x.Name)).ToImmutableArray(),
        };

        Counters.AddHits();
        return result.Cast<T>();
    }

    public Task<Option> Set<T>(string key, T value, ScopeContext context) => new Option(StatusCode.Conflict).ToTaskResult();
}
