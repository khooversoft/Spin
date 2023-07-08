using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Types;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Key;

public class PrincipalKeyClient
{
    private readonly HttpClient _client;
    public PrincipalKeyClient(HttpClient client) => _client = client;

    public async Task<Option> Create(ObjectId id, PrincipalKeyRequest content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.ApiPath.PrincipalKey}/create/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();

    public Task<Option> Delete(ObjectId id, ScopeContext context) => _client.Delete(SpinConstants.ApiPath.PrincipalKey, id, context);
    public Task<Option> Exist(ObjectId id, ScopeContext context) => _client.Exist(SpinConstants.ApiPath.PrincipalKey, id, context);
    public Task<Option<PrincipalKeyModel>> Get(ObjectId id, ScopeContext context) => _client.Get<PrincipalKeyModel>(SpinConstants.ApiPath.PrincipalKey, id, context);

    public async Task<Option> Update(ObjectId id, PrincipalKeyRequest content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.ApiPath.PrincipalKey}/{id}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .ToOption();
}
