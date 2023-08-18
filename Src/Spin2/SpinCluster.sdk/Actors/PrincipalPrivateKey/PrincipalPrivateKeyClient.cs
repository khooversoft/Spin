using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.PrincipalPrivateKey;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Actors.User;
using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.PrincipalKey;

public class PrincipalPrivateKeyClient
{
    protected readonly HttpClient _client;
    public PrincipalPrivateKeyClient(HttpClient client) => _client = client.NotNull();

    public async Task<Option> Delete(string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.PrincipalPrivateKey}/{Uri.EscapeDataString(principalId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .DeleteAsync(context)
        .ToOption();

    public async Task<Option<PrincipalPrivateKeyModel>> Get(string principalId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.PrincipalPrivateKey}/{Uri.EscapeDataString(principalId)}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<PrincipalPrivateKeyModel>();

    public async Task<Option> Set(PrincipalPrivateKeyModel content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.Schema.PrincipalPrivateKey}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();
}
