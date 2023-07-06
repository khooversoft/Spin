using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.ActorBase;
using SpinCluster.sdk.Actors.Tenant;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Types;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Key;

public class PrincipalKeyConnector : ConnectorBase<PrincipalKeyModel, IPrincipalKeyActor>
{
    public PrincipalKeyConnector(IClusterClient client, ILogger<PrincipalKeyConnector> logger)
        : base(client, "principalKey", logger)
    {
    }

    public override RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder builder = base.Setup(app);

        builder.MapPost("/create", async (PrincipalKeyRequest model, [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId) => (await Create(model, traceId)).ToResult());

        return builder;
    }

    private async Task<SpinResponse> Create(PrincipalKeyRequest model, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);
        return await _client.GetGrain<IPrincipalKeyActor>(model.KeyId).Create(model, context.TraceId);
    }
}
