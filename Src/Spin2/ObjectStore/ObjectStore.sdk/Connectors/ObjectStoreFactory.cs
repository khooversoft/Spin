using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ObjectStore.sdk.Application;
using Toolbox.Azure.DataLake;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Types.Maybe;

namespace ObjectStore.sdk.Connectors;

public class ObjectStoreFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ObjectStoreOption _option;
    private readonly IReadOnlyDictionary<string, DomainProfileOption> _profiles;
    private readonly ConcurrentDictionary<string, IDatalakeStore> _stores;
    private readonly ILogger<ObjectStoreFactory> _logger;

    public ObjectStoreFactory(ObjectStoreOption option, IServiceProvider serviceProvider, ILogger<ObjectStoreFactory> logger)
    {
        _serviceProvider = serviceProvider.NotNull();
        _option = option.NotNull();
        _logger = logger.NotNull();

        _profiles = _option.DomainProfiles.ToDictionary(x => x.DomainName, x => x);
        _stores = new ConcurrentDictionary<string, IDatalakeStore>(StringComparer.OrdinalIgnoreCase);
    }

    public Option<IDatalakeStore> Get(string domain)
    {
        domain.NotEmpty();

        if (!_profiles.TryGetValue(domain, out DomainProfileOption? profile))
            return new Option<IDatalakeStore>(StatusCode.NotFound);

        IDatalakeStore store = _stores.GetOrAdd(domain, x =>
        {
            _logger.LogInformation("Adding domain={domain}", domain);

            var option = new DatalakeOption
            {
                AccountName = profile.AccountName,
                ContainerName = profile.ContainerName,
                Credentials = _option.ClientIdentity,
                BasePath = profile.BasePath,
            };

            return ActivatorUtilities.CreateInstance<DatalakeStore>(_serviceProvider, option);
        });

        return new Option<IDatalakeStore>(store);
    }
}
