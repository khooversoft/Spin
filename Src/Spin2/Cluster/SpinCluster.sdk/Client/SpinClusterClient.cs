using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Client;

public class SpinClusterClient
{
    private readonly HttpClient _client;
    public SpinClusterClient(HttpClient client) => _client = client.NotNull();

    public async Task<StatusCode> Delete(ObjectId id, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/data/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .GetAsync(context)
        .GetStatusCode();

    public async Task<Option<T>> Get<T>(ObjectId id, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/data/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<T>();

    public async Task<StatusCode> Set<T>(ObjectId id, T content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/data/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .GetStatusCode();
}

