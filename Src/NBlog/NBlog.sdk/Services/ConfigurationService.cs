using Microsoft.Extensions.Logging;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public class ConfigurationService
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<ConfigurationService> _logger;
    private readonly StorageOption _storageOption;

    public ConfigurationService(IClusterClient clusterClient, StorageOption storageOption, ILogger<ConfigurationService> logger)
    {
        _clusterClient = clusterClient.NotNull();
        _storageOption = storageOption.NotNull();
        _logger = logger.NotNull();
    }

    public Task<IReadOnlyList<IndexGroup>> Lookup(string db, IReadOnlyList<string> groupNames, ScopeContext context)
    {
        return _clusterClient.GetConfigurationActor(db).Lookup(groupNames, context.TraceId);
    }

    public async Task<Option<NBlogConfiguration>> Get(string dbName, ScopeContext context)
    {
        if (dbName != "*")
        {
            var lookup = await _clusterClient.GetConfigurationActor(dbName).Get(context.TraceId);
            if (lookup.IsOk()) return lookup;

            _logger.LogWarning("Failed to find dbName={dbName}, trying defaultDbName={defaultDbName}", dbName, _storageOption.DefaultDbName);
        }

        var lookup2 = await _clusterClient.GetConfigurationActor(_storageOption.DefaultDbName.NotEmpty()).Get(context.TraceId);
        if (lookup2.IsError())
        {
            _logger.LogError("Failed to find defaultDbName={defaultDbName}", _storageOption.DefaultDbName);
        }

        return lookup2;
    }

    public async Task<Option<(string dbName, NBlogConfiguration config)>> GetOrDefault(string dbName, ScopeContext context)
    {
        if (dbName != "*")
        {
            var lookup = await _clusterClient.GetConfigurationActor(dbName).Get(context.TraceId);
            if (lookup.IsOk()) return (dbName, lookup.Return());

            _logger.LogWarning("Failed to find dbName={dbName}, trying defaultDbName={defaultDbName}", dbName, _storageOption.DefaultDbName);
        }

        var lookup2 = await _clusterClient.GetConfigurationActor(_storageOption.DefaultDbName.NotEmpty()).Get(context.TraceId);
        if (lookup2.IsError())
        {
            _logger.LogError("Failed to find defaultDbName={defaultDbName}", _storageOption.DefaultDbName);
        }

        return (_storageOption.DefaultDbName, lookup2.Return());
    }

}
