using SpinCluster.sdk.Application;
using SpinCluster.sdk.Types;
using Toolbox.Rest;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.ActorBase;

public static class ClientBaseExtensions
{
    public static async Task<Option> Delete(this HttpClient client, string rootPath, ObjectId id, ScopeContext context) => await new RestClient(client)
        .SetPath($"/{rootPath}/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .GetAsync(context)
        .ToOption();

    public static async Task<Option<T>> Get<T>(this HttpClient client, string rootPath, ObjectId id, ScopeContext context) => await new RestClient(client)
        .SetPath($"/{rootPath}/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<T>();

    public static async Task<Option> Set<T>(this HttpClient client, string rootPath, ObjectId id, T content, ScopeContext context) => await new RestClient(client)
        .SetPath($"/{rootPath}/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();

    public static async Task<Option> Exist(this HttpClient client, string rootPath, ObjectId id, ScopeContext context) => await new RestClient(client)
        .SetPath($"/{rootPath}/exist/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .GetAsync(context)
        .ToOption();
}