using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using SpinCluster.sdk.Actors.Search;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Types;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Configuration;

internal class TenantListAgent
{
    private readonly Func<ISearchActor> _getActor;
    private readonly CacheObject<IReadOnlyList<string>> _cache = new CacheObject<IReadOnlyList<string>>(TimeSpan.FromMinutes(15));
    public TenantListAgent(Func<ISearchActor> getActor) => _getActor = getActor.NotNull();

    public async Task<IReadOnlyList<string>> Get(ScopeContext context)
    {
        if (_cache.TryGetValue(out IReadOnlyList<string> value)) return value;

        SearchQuery searchQuery = new SearchQuery
        {
            Schema = SpinConstants.Schema.Tenant,
            Tenant = SpinConstants.Schema.System,
            Recurse = true,
        };

        SpinResponse<IReadOnlyList<StorePathItem>> tenantList = await _getActor()
            .Search(searchQuery, context.TraceId);

        IReadOnlyList<StorePathItem> customTenants = tenantList.Return();

        IReadOnlyList<string> result = customTenants
            .Where(x => x.Name != SpinConstants.Schema.System)
            .Select(x => x.Name.Replace("$system/", string.Empty))
            .ToArray();

        _cache.Set(result);
        return result;
    }
}
