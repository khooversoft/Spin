using SpinCluster.sdk.Actors.Configuration;
using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Client;

public  class SpinConfigurationClient
{
    private readonly HttpClient _client;
    public SpinConfigurationClient(HttpClient client) => _client = client.NotNull();

    public async Task<Option<SiloConfigOption>> Get(ScopeContext context) => await new RestClient(_client)
        .SetPath("configuration")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<SiloConfigOption>();

    public async Task<StatusCode> Set(SiloConfigOption request, string leaseId, ScopeContext context) => await new RestClient(_client)
        .SetPath("configuration")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .SetContent(request)
        .PostAsync(context)
        .GetStatusCode();
}
