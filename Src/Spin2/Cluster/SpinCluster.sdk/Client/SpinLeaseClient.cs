using SpinCluster.sdk.Actors.Lease;
using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Client;

public class SpinLeaseClient
{
    private readonly HttpClient _client;
    public SpinLeaseClient(HttpClient client) => _client = client.NotNull();

    public async Task<Option<LeaseData>> Acquire(ObjectId id, ScopeContext context) => await new RestClient(_client)
        .SetPath($"lease/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<LeaseData>();

    public async Task<StatusCode> Release(ObjectId id, string leaseId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"lease/{leaseId}/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .DeleteAsync(context)
        .GetStatusCode();
}
