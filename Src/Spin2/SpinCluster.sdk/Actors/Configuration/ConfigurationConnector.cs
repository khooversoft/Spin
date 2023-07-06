using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Resource;
using SpinCluster.sdk.Types;
using Toolbox.Tools.Zip;
using Toolbox.Tools;
using Toolbox.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using SpinCluster.sdk.Application;
using Microsoft.AspNetCore.Http;
using Toolbox.Azure.DataLake;
using SpinCluster.sdk.Actors.Search;
using System.Reflection;
using SpinCluster.sdk.Actors.Lease;

namespace SpinCluster.sdk.Actors.Configuration;

internal class ConfigurationConnector
{
    private readonly IClusterClient _client;
    private readonly ILogger<ConfigurationConnector> _logger;
    private const string _configKey = "$system";

    public ConfigurationConnector(IClusterClient client, ILogger<ConfigurationConnector> logger)
    {
        _client = client.NotNull();
        _logger = logger.NotNull();
    }

    public void Setup(IEndpointRouteBuilder app)
    {
        app.MapGet("/configuration", async ([FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId) =>
        {
            var response = await Get(traceId);
            return response.ToResult();
        });

        app.MapPost("/configuration", async (SiloConfigOption request, [FromHeader(Name = SpinConstants.Protocol.TraceId)] string traceId) =>
        {
            var response = await Set(request, traceId);
            return response.ToResult();
        });
    }

    public async Task<SpinResponse<SiloConfigOption>> Get(string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        var response = await _client.GetGrain<IConfigurationActor>(_configKey).Get(context.TraceId);
        return response;
    }

    public async Task<SpinResponse> Set(SiloConfigOption request, string traceId)
    {
        var context = new ScopeContext(traceId, _logger);

        IConfigurationActor actor = _client.GetGrain<IConfigurationActor>(_configKey);
        return await actor.Set(request, context.TraceId);
    }
}
