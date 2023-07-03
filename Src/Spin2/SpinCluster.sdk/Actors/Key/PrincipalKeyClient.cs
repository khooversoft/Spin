using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Application;
using Toolbox.Rest;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Key;

public class PrincipalKeyClient
{
    private readonly HttpClient _client;
    public PrincipalKeyClient(HttpClient client) => _client = client.NotNull();

    public async Task<Option<StatusResponse>> Delete(string keyId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.ApiPath.PrincipalKey}/{keyId}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<StatusResponse>();

    public async Task<Option<PrincipalKeyModel>> Get(string keyId, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.ApiPath.PrincipalKey}/{keyId}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .GetAsync(context)
        .GetContent<PrincipalKeyModel>();

    public async Task<Option<StatusResponse>> Create(PrincipalKeyRequest model, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{SpinConstants.ApiPath.PrincipalKey}/{model.KeyId}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .SetContent(model)
        .PostAsync(context)
        .GetContent<StatusResponse>();

}
