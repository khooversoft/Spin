using Microsoft.Extensions.Logging;
using SpinCluster.abstraction;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClient.sdk;

public class TenantClient
{
    protected readonly HttpClient _client;
    private readonly ILogger<TenantClient> _logger;

    public TenantClient(HttpClient client, ILogger<TenantClient> logger)
    {
        _client = client.NotNull();
        _logger = logger;
    }

    public async Task<Option> Delete(string tenantId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Tenant}/{Uri.EscapeDataString(tenantId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context.With(_logger))
        .ToOption();

    public async Task<Option<TenantModel>> Get(string tenantId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Tenant}/{Uri.EscapeDataString(tenantId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context.With(_logger))
        .GetContent<TenantModel>();

    public async Task<Option> Set(TenantModel content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Tenant}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context.With(_logger))
        .ToOption();
}
