using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Actors.Search;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.ActorBase;

public abstract class ClientBase<T>
{
    private readonly HttpClient _client;
    private readonly string _rootPath;
    public ClientBase(HttpClient client, string rootPath) => (_client, _rootPath) = (client.NotNull(), rootPath.NotEmpty());

    public async Task<Option<StatusResponse>> Delete(ObjectId id, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{_rootPath}/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<StatusResponse>();

    public async Task<Option<T>> Get(ObjectId id, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{_rootPath}/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<T>();

    public async Task<Option<StatusResponse>> Set(ObjectId id, T content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{_rootPath}/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .GetContent<StatusResponse>();

    public async Task<Option<QueryResponse<T>>> Search(QueryParameter query, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{_rootPath}/search?{query.ToQueryString()}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<QueryResponse<T>>();
}
