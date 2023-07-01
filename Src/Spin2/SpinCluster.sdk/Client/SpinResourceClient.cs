using SpinCluster.sdk.Actors.Resource;
using SpinCluster.sdk.Actors.Search;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Client;

public class SpinResourceClient
{
    private readonly HttpClient _client;
    public SpinResourceClient(HttpClient client) => _client = client.NotNull();

    public Task<Option<IReadOnlyList<StorePathItem>>> Search(QueryParameter queryParameter, ScopeContext context)
    {
        return Search(queryParameter.Filter, queryParameter.Index, queryParameter.Count, queryParameter.Recurse, context);
    }

    public async Task<Option<IReadOnlyList<StorePathItem>>> Search(string? filter, int? index, int? count, bool? recurse, ScopeContext context)
    {
        string query = new string?[]
        {
            index?.ToString()?.Func(x => $"index={x}"),
            count?.ToString()?.Func(x => $"count={x}"),
            recurse?.ToString()?.Func(x => $"recurse={x}"),
        }
        .Where(x => x != null)
        .Join("&")
        .Func(x => x.IsNotEmpty() ? "?" + x : string.Empty);

        return await new RestClient(_client)
            .SetPath($"/search/{filter}{query}")
            .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
            .GetAsync(context)
            .GetContent<IReadOnlyList<StorePathItem>>();
    }

    public async Task<StatusCode> Delete(ObjectId id, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/resource/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .GetAsync(context)
        .GetStatusCode();

    public async Task<Option<ResourceFile>> Get(ObjectId id, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/resource/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<ResourceFile>();

    public async Task<StatusCode> Set(ObjectId id, ResourceFile content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/resource/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .GetStatusCode();
}
