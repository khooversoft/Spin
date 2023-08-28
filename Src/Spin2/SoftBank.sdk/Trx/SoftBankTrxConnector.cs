using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SoftBank.sdk.Application;
using SoftBank.sdk.Models;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.Lease;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SoftBank.sdk.Trx;

public class SoftBankTrxConnector
{
    protected readonly IClusterClient _client;
    protected readonly ILogger<LeaseConnector> _logger;

    public SoftBankTrxConnector(IClusterClient client, ILogger<LeaseConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{IdSoftbank.SoftBankTrxSchema}");

        group.MapPost("/", Request);

        return group;
    }

    private async Task<IResult> Request(TrxRequest request, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        var v = request.Validate();
        if (v.IsError()) return v.ToResult();

        ResourceId trxActorKey = ((ResourceId)request.SourceAccountID).ToSoftBankTrxId();
        Option<TrxResponse> response = await _client.GetResourceGrain<ISoftBankTrxActor>(trxActorKey).Request(request, traceId);

        return response.ToResult();
    }
}
