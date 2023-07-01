using Microsoft.AspNetCore.Mvc;
using SpinCluster.sdk.Actors.Configuration;
using SpinCluster.sdk.Actors.Lease;
using SpinCluster.sdk.Application;
using SpinCluster.sdk.Types;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinClusterApi.Connectors;

internal class ConfigurationConnector
{
    private readonly IClusterClient _client;
    private readonly ILogger<LeaseConnector> _logger;
    private const string _configKey = "$system";

    public ConfigurationConnector(IClusterClient client, ILogger<LeaseConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public void Setup(IEndpointRouteBuilder app)
    {
        //var group = app.MapGroup("/configuration");

        app.MapGet("/configuration", async ([FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId) => await Get(traceId) switch
        {
            var v when v.IsError() => Results.StatusCode((int)v.StatusCode.ToHttpStatusCode()),
            var v when v.HasValue => Results.Ok(v.Return()),
            var v => Results.BadRequest(v.Return()),
        });

        app.MapPost("/configuration", async (SiloConfigOption request, [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId) =>
        {
            StatusCode statusCode = await Set(request, traceId);
            return Results.StatusCode((int)statusCode.ToHttpStatusCode());
        });
    }

    public async Task<Option<object>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        IConfigurationActor actor = _client.GetGrain<IConfigurationActor>(_configKey);
        SpinResponse<SiloConfigOption> response = await actor.Get(context.TraceId);

        if (response.StatusCode.IsError()) return response.StatusCode.ToOption<object>();
        return response.Return();
    }

    public async Task<StatusCode> Set(SiloConfigOption request, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        IConfigurationActor actor = _client.GetGrain<IConfigurationActor>(_configKey);
        return await actor.Set(request, context.TraceId);
    }
}
