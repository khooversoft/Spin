using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using SpinCluster.sdk.Services;
using Toolbox.Azure.DataLake;
using Toolbox.Extensions;
using Toolbox.Orleans.Types;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Search;

public interface ISearchActor : IGrainWithStringKey
{
    Task<Option<IReadOnlyList<StorePathItem>>> Search(SearchQuery searchQuery, string traceId);
    Task<Option> Exist(string objectId, string traceId);
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

    public async Task<Option<IReadOnlyList<StorePathItem>>> Search(SearchQuery searchQuery, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        var validationResult = _validator.Validate(searchQuery).LogResult(context.Location());
        if(validationResult.IsError()) return validationResult.ToOptionStatus<IReadOnlyList<StorePathItem>>();

        Option<IDatalakeStore> store = _datalakeResources.GetStore(searchQuery.Schema, context.Location());
        if (store.IsError()) return store.ToOptionStatus<IReadOnlyList<StorePathItem>>();

        Option<QueryResponse<DatalakePathItem>> result = await store.Return().Search(searchQuery.ConvertTo(), context);
        if (result.IsError())
        {
            context.Location().LogError("Failed to search datalake store for searchQuery={searchQuery}", searchQuery);
            return new Option<IReadOnlyList<StorePathItem>>(StatusCode.BadRequest);
        }

        return result.Return().Items
            .Select(x => x.ConvertTo())
            .ToArray();
    }

    public async Task<Option> Exist(string objectId, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        var objId = ObjectId.Create(objectId);
        if (objId.IsError()) return objId.ToOptionStatus();

        Option<IDatalakeStore> store = _datalakeResources.GetStore(objId.Return().Schema, context.Location());
        if (store.IsError()) return store.ToOptionStatus();

        StatusCode statusCode = await store.Return().Exist(objId.Return().FilePath, context);
        return new Option(statusCode);
    }
}
