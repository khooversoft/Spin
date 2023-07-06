using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Actors.Search;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Types;
using Toolbox.Extensions;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.ActorBase;

public abstract class ClientBase<T>
{
    protected readonly HttpClient _client;
    protected private readonly string _rootPath;
    public ClientBase(HttpClient client, string rootPath) => (_client, _rootPath) = (client.NotNull(), rootPath.NotEmpty());

    public virtual async Task<Option<Option>> Delete(ObjectId id, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{_rootPath}/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .GetAsync(context)
        .ToOption();

    public virtual async Task<Option<T>> Get(ObjectId id, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{_rootPath}/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .GetAsync(context)
        .ToOption<T>();

    public virtual async Task<Option> Set(ObjectId id, T content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{_rootPath}/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();

    public virtual async Task<Option> Exist(ObjectId id, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{_rootPath}/exist/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .GetAsync(context)
        .ToOption();
}
