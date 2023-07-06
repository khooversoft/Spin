using SpinCluster.sdk.Application;
using SpinCluster.sdk.Types;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Configuration;

public class SpinConfigurationClient
{
    private readonly HttpClient _client;
    public SpinConfigurationClient(HttpClient client) => _client = client.NotNull();

    public async Task<SpinResponse<SiloConfigOption>> Get(ScopeContext context) => await new RestClient(_client)
        .SetPath("configuration")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<SpinResponse<SiloConfigOption>>()
        .UnwrapAsync();

    public async Task<SpinResponse> Set(SiloConfigOption request, string leaseId, ScopeContext context) => await new RestClient(_client)
        .SetPath("configuration")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .SetContent(request)
        .PostAsync(context)
        .GetContent<SpinResponse>()
        .UnwrapAsync();
}
