using Microsoft.AspNetCore.Mvc;
using SpinClusterApi.Connectors;
using Toolbox.Types;

namespace SpinClusterApi.Application;

internal static class Startup
{
    public static IServiceCollection AddSpinApi(this IServiceCollection services)
    {
        services.AddSingleton<ActorConnector>();

        return services;
    }

    public static void MapSpinApi(this IEndpointRouteBuilder app)
    {
        ActorConnector router = app.ServiceProvider.GetRequiredService<ActorConnector>();

        app.MapGet("/{*objectId}", async (string objectId, [FromHeader(Name = "spin-trace-id")] string traceId) => await router.Get(objectId, traceId) switch
        {
            var v when v.IsError() => Results.StatusCode((int)v.StatusCode.ToHttpStatusCode()),
            var v when v.HasValue => Results.Ok(v.Return()),
            var v => Results.BadRequest(v.Return()),
        });

        app.MapPost("/{*objectId}", async (string objectId, [FromHeader(Name = "spin-trace-id")] string traceId, HttpRequest request) =>
        {
            string body = "";
            using (StreamReader stream = new StreamReader(request.Body))
            {
                body = await stream.ReadToEndAsync();
            }

            StatusCode statusCode = await router.Set(objectId, traceId, body);
            return Results.StatusCode((int)statusCode.ToHttpStatusCode());
        });

        app.MapDelete("/{*objectId}", async (string objectId, [FromHeader(Name = "spin-trace-id")] string traceId) =>
        {
            StatusCode statusCode = await router.Delete(objectId, traceId);
            return Results.StatusCode((int)statusCode.ToHttpStatusCode());
        });
    }
}
