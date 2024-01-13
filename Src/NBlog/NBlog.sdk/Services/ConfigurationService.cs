using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public class ConfigurationService
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<ConfigurationService> _logger;

    public ConfigurationService(IClusterClient clusterClient, ILogger<ConfigurationService> logger)
    {
        _clusterClient = clusterClient.NotNull();
        _logger = logger.NotNull();
    }

    public Task<IReadOnlyList<IndexGroup>> Lookup(string db, IReadOnlyList<string> groupNames, ScopeContext context)
    {
        return _clusterClient.GetConfigurationActor(db).Lookup(groupNames, context.TraceId);
    }

    public async Task<NBlogConfiguration> Get(string dbName, ScopeContext context)
    {
        var option = await _clusterClient.GetConfigurationActor(dbName).Get(context.TraceId);
        if (option.IsError()) throw new InvalidOperationException($"Failed to get configuration for db={dbName}");
        return option.Return();
    }
}
