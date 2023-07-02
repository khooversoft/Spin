﻿using SpinCluster.sdk.Actors.Search;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;

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
        Search = new SearchClient(client);
        User = new UserClient(client);
    }

    public SpinConfigurationClient Configuration { get; }
    public SpinDataClient Data { get; }
    public SpinLeaseClient Lease { get; }
    public SpinResourceClient Resource { get; }
    public TenantClient Tenant { get; }
    public SearchClient Search { get; }
    public UserClient User { get; }
}
