using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.ActorBase;

public static class ClientBaseExtensions
{
    public static async Task<Option> Delete(this HttpClient client, string rootPath, ObjectId id, string? principalId, ScopeContext context) => await new RestClient(client)
        .SetPath($"/{rootPath}/{id}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .AddHeader(SpinConstants.Headers.PrincipalId, principalId?.ToString())
        .DeleteAsync(context)
        .ToOption();

    public static async Task<Option<T>> Get<T>(this HttpClient client, string rootPath, ObjectId id, string? principalId, ScopeContext context) => await new RestClient(client)
        .SetPath($"/{rootPath}/{id}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .AddHeader(SpinConstants.Headers.PrincipalId, principalId?.ToString())
        .GetAsync(context)
        .GetContent<T>();

    public static async Task<Option> Set<T>(this HttpClient client, string rootPath, ObjectId id, T content, string? principalId, ScopeContext context) => await new RestClient(client)
        .SetPath($"/{rootPath}/{id}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .AddHeader(SpinConstants.Headers.PrincipalId, principalId?.ToString())
        .SetContent(content)
        .PostAsync(context)
        .ToOption();

    public static async Task<Option> Exist(this HttpClient client, string rootPath, ObjectId id, string? principalId, ScopeContext context) => await new RestClient(client)
        .SetPath($"/{rootPath}/exist/{id}")
        .AddHeader(SpinConstants.Headers.TraceId, context.TraceId)
        .AddHeader(SpinConstants.Headers.PrincipalId, principalId?.ToString())
        .GetAsync(context)
        .ToOption();
}