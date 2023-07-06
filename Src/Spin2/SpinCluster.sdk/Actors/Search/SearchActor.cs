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
    private readonly DatalakeSchemaResources _datalakeResources;
    private readonly ILogger<SearchActor> _logger;
    private readonly IValidator<SearchQuery> _validator;

    public SearchActor(DatalakeSchemaResources datalakeResources, IValidator<SearchQuery> validator, ILogger<SearchActor> logger)
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

        Option<IDatalakeStore> store = _datalakeResources.GetStore(searchQuery.Schema);
        if (store.IsError())
        {
            context.Location().LogError("Failed to get datalake store for schemaName={schemaName}", searchQuery.Schema);
            return new SpinResponse<IReadOnlyList<StorePathItem>>(StatusCode.BadRequest, "Failed to get schema");
        }

        Option<QueryResponse<DatalakePathItem>> result = await store.Return().Search(searchQuery.ConvertTo(), context);
        if (result.IsError())
        {
            context.Location().LogError("Failed to search datalake store for searchQuery={searchQuery}", searchQuery);
            return new SpinResponse<IReadOnlyList<StorePathItem>>(StatusCode.BadRequest);
        }

        return result.Return().Items
            .Select(x => x.ConvertTo())
            .ToArray();
    }
}
