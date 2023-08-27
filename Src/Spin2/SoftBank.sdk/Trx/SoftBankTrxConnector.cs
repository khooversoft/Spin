using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.Lease;
using SpinCluster.sdk.Application;
using static Microsoft.Azure.Amqp.Serialization.SerializableType;
using Toolbox.Tools;
using Toolbox.Types;
using Microsoft.AspNetCore.Builder;

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
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.Lease}");

        group.MapPost("/{leaseId}", Acquire);

        return group;
    }

    private async Task<IResult> Acquire(string leaseId, LeaseCreate model, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        leaseId = Uri.UnescapeDataString(leaseId);
        if (!IdPatterns.IsLeaseId(leaseId)) return Results.BadRequest();

        Option<LeaseData> response = await _client.GetLeaseActor(leaseId).Acquire(model, traceId);
        return response.ToResult();
    }
}
