using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using SpinCluster.sdk.Services;
using SpinCluster.sdk.Types;
using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Search;

public interface ISearchActor : IGrainWithStringKey
{
    Task<SpinResponse<IReadOnlyList<StorePathItem>>> Search(SearchQuery searchQuery, string traceId);
}

[StatelessWorker]
[Reentrant]
public class SearchActor : Grain, ISearchActor
{
    private readonly DatalakeResources _datalakeResources;
    private readonly ILogger<SearchActor> _logger;
    private readonly Validator<SearchQuery> _validator;

    public SearchActor(DatalakeResources datalakeResources, Validator<SearchQuery> validator, ILogger<SearchActor> logger)
    {
        _datalakeResources = datalakeResources.NotNull();
        _logger = logger.NotNull();
        _validator = validator;
    }

    public async Task<SpinResponse<IReadOnlyList<StorePathItem>>> Search(SearchQuery searchQuery, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        switch (_validator.Validate(searchQuery))
        {
            case var v when !v.IsValid:
                return new SpinResponse<IReadOnlyList<StorePathItem>>(StatusCode.BadRequest, v.FormatErrors());
        }

        string[] parts = searchQuery.Filter.Split('/', StringSplitOptions.RemoveEmptyEntries);
        string schema = parts[0];
        string filter = parts.Skip(1).Join('/');

        Option<IDatalakeStore> store = _datalakeResources.GetStore(schema);
        if (store.IsError())
        {
            context.Location().LogError("Failed to get datalake store for schemaName={schemaName}", schema);
            return new SpinResponse<IReadOnlyList<StorePathItem>>(StatusCode.BadRequest, "Failed to get schema");
        }

        searchQuery = searchQuery with { Filter = filter };

        var result = await store.Return().Search(searchQuery.ConvertTo(), context);
        if (result.IsError())
        {
            context.Location().LogError("Failed to search datalake store for searchQuery={searchQuery}", searchQuery);
            return new SpinResponse<IReadOnlyList<StorePathItem>>(StatusCode.BadRequest);
        }

        return result.Return()
            .Select(x => x.ConvertTo())
            .ToArray()
            .ToSpinResponse<IReadOnlyList<StorePathItem>>();
    }
}
