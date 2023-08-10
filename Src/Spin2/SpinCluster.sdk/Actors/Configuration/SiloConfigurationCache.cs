using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Services;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Configuration;

internal class SiloConfigurationCache
{
    private readonly SiloConfigStore _configStore;
    private readonly CacheObject<SiloConfigOption> _cache = new CacheObject<SiloConfigOption>(TimeSpan.FromMinutes(15));

    public SiloConfigurationCache(SiloConfigStore configStore) => _configStore = configStore.NotNull();

    public async Task<Option<SiloConfigOption>> Get(ScopeContext context)
    {
        context.Location().LogInformation("Getting SpinConfigruation");

        if (_cache.TryGetValue(out SiloConfigOption value)) return value;

        SiloConfigOption option = await _configStore.Get(context).Return();
        _cache.Set(option);
        return option;
    }
}
