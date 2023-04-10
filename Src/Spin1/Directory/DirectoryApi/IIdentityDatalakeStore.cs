using Toolbox.Azure.DataLake;
using Toolbox.Azure.DataLake.Model;

namespace DirectoryApi;

public interface IIdentityDatalakeStore : IDatalakeStore
{
}

public class IdentityDatalakeStore : DatalakeStore, IIdentityDatalakeStore
{
    public IdentityDatalakeStore(DatalakeStoreOption azureStoreOption, ILogger<DatalakeStore> logger)
        : base(azureStoreOption, logger)
    {
    }
}
