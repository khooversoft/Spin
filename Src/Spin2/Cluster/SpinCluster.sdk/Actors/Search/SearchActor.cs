using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Services;
using SpinCluster.sdk.Types;
using Toolbox.Azure.DataLake;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Search;

public interface ISearchActor : IGrainWithStringKey
{
    Task<SpinResponse<IReadOnlyList<StorePathItem>>> Search(SearchQuery searchQuery, ScopeContext context);
}

public class SearchActor : Grain, ISearchActor
{
    private readonly DatalakeResources _datalakeResources;
    private readonly ILogger<SearchActor> _logger;

    public SearchActor(DatalakeResources datalakeResources, ILogger<SearchActor> logger)
    {
        _datalakeResources = datalakeResources.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<SpinResponse<IReadOnlyList<StorePathItem>>> Search(SearchQuery searchQuery, ScopeContext context)
    {
        context = context.With(_logger);

        string schemaName = this.GetPrimaryKeyString();
        Option<IDatalakeStore> store = _datalakeResources.GetStore(schemaName);
        if (store.IsError())
        {
            context.Location().LogError("Failed to get datalake store for schemaName={schemaName}", schemaName);
            return new SpinResponse<IReadOnlyList<StorePathItem>>(StatusCode.BadRequest);
        }

        var result = await store.Return().Search(searchQuery.ConvertTo(), context);
        if (result.IsError())
        {
            context.Location().LogError("Failed to search datalake store for schemaName={schemaName}", schemaName);
            return new SpinResponse<IReadOnlyList<StorePathItem>>(StatusCode.BadRequest);
        }

        return result.Return()
            .Select(x => x.ConvertTo(schemaName))
            .ToArray()
            .ToSpinResponse<IReadOnlyList<StorePathItem>>();
    }
}
