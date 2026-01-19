using Microsoft.Extensions.Logging;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Graph;

public class SecurityPartition
{
    private readonly IKeyStore<SecurityPartitionData> _keyStore;
    private readonly ILogger _logger;

    public SecurityPartition(IKeyStore<SecurityPartitionData> keyStore, ILogger logger)
    {
        _keyStore = keyStore.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option<SecurityPartitionData>> Get()
    {
        var getOption = await _keyStore.Get(GraphConstants.GraphSecurity.Key);
        if (getOption.IsError()) _logger.LogError("Failed to get security partition data from key={key}", GraphConstants.GraphMap.Key);

        return getOption;
    }

    public async Task<Option<string>> Set(SecurityPartitionData data)
    {
        var setOption = await _keyStore.Set(GraphConstants.GraphSecurity.Key, data);
        if (setOption.IsError()) _logger.LogError("Failed to set security partition data from key={key}", GraphConstants.GraphMap.Key);

        return setOption;
    }
}


public record SecurityPartitionData
{
    public IReadOnlyList<GroupPolicy> SecurityGroups { get; init; } = Array.Empty<GroupPolicy>();
    public IReadOnlyList<PrincipalIdentity> PrincipalIdentities { get; init; } = Array.Empty<PrincipalIdentity>();
}