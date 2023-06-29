using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace SpinCluster.sdk.Client;

public class SpinClusterClient
{
    public SpinClusterClient(HttpClient client)
    {
        Configuration = new SpinConfigurationClient(client);
        Data = new SpinDataClient(client);
        Lease = new SpinLeaseClient(client);
        Resource = new SpinResourceClient(client);
    }

    public SpinConfigurationClient Configuration { get; }
    public SpinDataClient Data { get; }
    public SpinLeaseClient Lease { get; }
    public SpinResourceClient Resource { get; }
}
