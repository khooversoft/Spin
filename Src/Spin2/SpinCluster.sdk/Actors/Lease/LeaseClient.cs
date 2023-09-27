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

    public async Task<Option> Acquire(LeaseData model, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Lease}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(model)
        .PostAsync(context.With(_logger))
        .ToOption();

    public async Task<Option<LeaseData>> Get(string leaseKey, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Lease}/{Uri.EscapeDataString(leaseKey)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<LeaseData>();

    public async Task<Option> IsValid(string leaseKey, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Lease}/{Uri.EscapeDataString(leaseKey)}/isValid")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .ToOption();

    public async Task<Option<IReadOnlyList<LeaseData>>> List(QueryParameter query, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Lease}/list")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(query)
        .PostAsync(context.With(_logger))
        .GetContent<IReadOnlyList<LeaseData>>();

    public async Task<Option> Release(string leaseKey, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Lease}/{Uri.EscapeDataString(leaseKey)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();
}
