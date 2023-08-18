using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.PrincipalKey;

public class PrincipalKeyClient
{
    protected readonly HttpClient _client;
    public PrincipalKeyClient(HttpClient client) => _client = client.NotNull();

    public async Task<Option> Delete(string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.PrincipalKey}/{Uri.EscapeDataString(principalId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context)
        .ToOption();

    public async Task<Option<PrincipalKeyModel>> Get(string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.PrincipalKey}/{Uri.EscapeDataString(principalId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<PrincipalKeyModel>();

    public async Task<Option> Create(PrincipalKeyCreateModel content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.PrincipalKey}/create")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();

    public async Task<Option> Update(PrincipalKeyModel content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.PrincipalKey}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();
}
