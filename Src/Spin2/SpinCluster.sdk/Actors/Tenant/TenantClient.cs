using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Tenant;

public class TenantClient
{
    protected readonly HttpClient _client;
    public TenantClient(HttpClient client) => _client = client.NotNull();

    public async Task<Option> Delete(string tenantId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Tenant}/{Uri.EscapeDataString(tenantId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context)
        .ToOption();

    public async Task<Option<TenantModel>> Get(string tenantId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Tenant}/{Uri.EscapeDataString(tenantId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<TenantModel>();

    public async Task<Option> Set(TenantModel content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Tenant}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();
}
