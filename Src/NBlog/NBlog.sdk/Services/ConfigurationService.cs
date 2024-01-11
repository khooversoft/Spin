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

    public Task<Option<NBlogConfiguration>> Get(string db, ScopeContext context) => _clusterClient.GetConfigurationActor(db).Get(context.TraceId);
}
