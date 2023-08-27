using SpinCluster.sdk.Actors.Lease;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Contract;

public class LeaseClient
{
    protected readonly HttpClient _client;
    public LeaseClient(HttpClient client) => _client = client.NotNull();

    public async Task<Option<LeaseData>> Acquire(string leaseId, LeaseCreate model, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Lease}/{Uri.EscapeDataString(leaseId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(model)
        .PostAsync(context)
        .GetContent<LeaseData>();

    public async Task<Option> Release(string leaseId, string leaseKey, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Lease}/{Uri.EscapeDataString(leaseId)}/{Uri.EscapeDataString(leaseKey)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context)
        .ToOption();

    public async Task<Option> IsValid(string leaseId, string leaseKey, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Lease}/{Uri.EscapeDataString(leaseId)}/isValid/{Uri.EscapeDataString(leaseKey)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context)
        .ToOption();

    public async Task<Option<IReadOnlyList<LeaseData>>> List(string leaseId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Lease}/{Uri.EscapeDataString(leaseId)}/list")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<IReadOnlyList<LeaseData>>();
}
