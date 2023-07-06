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

public class PrincipalKeyClient : ClientBase<PrincipalKeyModel>
{
    public PrincipalKeyClient(HttpClient client) : base(client, SpinConstants.ApiPath.PrincipalKey) { }

    public async Task<Option<SpinResponse>> Set(PrincipalKeyRequest content, ScopeContext context) => await new RestClient(_client)
        .SetPath($"/{_rootPath}")
        .AddHeader(SpinConstants.Protocol.TraceId, context.TraceId)
        .SetContent(content)
        .PostAsync(context)
        .GetContent<SpinResponse>();
}
