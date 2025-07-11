using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
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

    public async Task<Option<NBlogConfiguration>> Get(DbNameId dbNameId, ScopeContext context)
    {
        dbNameId = dbNameId.DbName switch
        {
            "*" => _storageOption.DefaultDbName,
            string v => v,
        };

        var lookup = await _clusterClient.GetConfigurationActor(dbNameId).Get(context.TraceId);
        if (lookup.IsError())
        {
            _logger.LogError("Failed to find dbName={dbName} and defaultDbName={defaultDbName}", dbNameId, _storageOption.DefaultDbName);
            return lookup;
        }

        return lookup;
    }
}
