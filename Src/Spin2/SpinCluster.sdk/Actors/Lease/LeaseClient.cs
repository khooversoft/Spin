using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Lease;
using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Contract;

public class LeaseClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<LeaseClient> _logger;

    public LeaseClient(HttpClient client, ILogger<LeaseClient> logger)
    {
        _client = client.NotNull();
        _logger = logger;
    }

    public async Task<Option<LeaseData>> Acquire(string leaseId, LeaseCreate model, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Lease}/{Uri.EscapeDataString(leaseId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(model)
        .PostAsync(context.With(_logger))
        .GetContent<LeaseData>();

    public async Task<Option> Release(string leaseId, string leaseKey, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Lease}/{Uri.EscapeDataString(leaseId)}/{Uri.EscapeDataString(leaseKey)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option> IsValid(string leaseId, string leaseKey, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Lease}/{Uri.EscapeDataString(leaseId)}/isValid/{Uri.EscapeDataString(leaseKey)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .ToOption();

    public async Task<Option<IReadOnlyList<LeaseData>>> List(string leaseId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Lease}/{Uri.EscapeDataString(leaseId)}/list")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<IReadOnlyList<LeaseData>>();
}
