using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Actors.Domain;

public class DomainConnector
{
    protected readonly IClusterClient _client;
    protected readonly ILogger<DomainConnector> _logger;

    public DomainConnector(IClusterClient client, ILogger<DomainConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public virtual RouteGroupBuilder Setup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup($"/{SpinConstants.Schema.Domain}");

        group.MapGet("/{domain}", GetDetails);
        group.MapGet("/list", List);
        group.MapPost("/{domain}/setExternalDomain", SetExternalDomain);
        group.MapPost("/{domain}/removeExternalDomain", RemoveExternalDomain);

        return group;
    }

    private async Task<IResult> GetDetails(string domain, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        domain = Uri.UnescapeDataString(domain);
        if (!IdPatterns.IsDomain(domain)) return Results.BadRequest("Invalid domain");

        Option<DomainDetail> response = await _client
            .GetResourceGrain<IDomainActor>(SpinConstants.DomainActorKey)
            .GetDetails(domain, traceId);

        return response.ToResult();
    }

    private async Task<IResult> List([FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        DomainList response = await _client
            .GetResourceGrain<IDomainActor>(SpinConstants.DomainActorKey)
            .List(traceId);

        return Results.Ok(response);
    }

    private async Task<IResult> SetExternalDomain(string domain, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        domain = Uri.UnescapeDataString(domain);
        if (!IdPatterns.IsDomain(domain)) return Results.BadRequest("Invalid domain");

        Option response = await _client
            .GetResourceGrain<IDomainActor>(SpinConstants.DomainActorKey)
            .SetExternalDomain(domain, traceId);

        return response.ToResult();
    }

    private async Task<IResult> RemoveExternalDomain(string domain, [FromHeader(Name = SpinConstants.Headers.TraceId)] string traceId)
    {
        domain = Uri.UnescapeDataString(domain);
        if (!IdPatterns.IsDomain(domain)) return Results.BadRequest("Invalid domain");

        Option response = await _client
            .GetResourceGrain<IDomainActor>(SpinConstants.DomainActorKey)
            .RemoveExternalDomain(domain, traceId);

        return response.ToResult();
    }
}
