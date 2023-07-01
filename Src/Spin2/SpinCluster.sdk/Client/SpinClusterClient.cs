using SpinCluster.sdk.Actors.Tenant;

namespace SpinCluster.sdk.Client;

public class SpinClusterClient
{
    public SpinClusterClient(HttpClient client)
    {
        Configuration = new SpinConfigurationClient(client);
        Data = new SpinDataClient(client);
        Lease = new SpinLeaseClient(client);
        Resource = new SpinResourceClient(client);
        Tenant = new TenantClient(client);
    }

    public SpinConfigurationClient Configuration { get; }
    public SpinDataClient Data { get; }
    public SpinLeaseClient Lease { get; }
    public SpinResourceClient Resource { get; }
    public TenantClient Tenant { get; }
}
