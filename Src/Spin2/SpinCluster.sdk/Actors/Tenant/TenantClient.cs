using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Subscription;
using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Tenant;

public class TenantClient
{
    protected readonly HttpClient _client;
    public TenantClient(HttpClient client) => _client = client.NotNull();

    public async Task<Option> Delete(TenantId tenantId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Tenant}/{tenantId.ToUrlEncoding()}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context)
        .ToOption();

    public async Task<Option<TenantModel>> Get(TenantId tenantId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.Tenant}/{tenantId.ToUrlEncoding()}")
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
