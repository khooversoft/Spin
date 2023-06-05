using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ObjectStore.sdk.Api;
using ObjectStore.sdk.Application;
using ObjectStore.sdk.Connectors;
using Toolbox.DocumentContainer;
using Toolbox.Types;

namespace ObjectStore.sdk;

public static class Startup
{
    public static IServiceCollection AddObjectStore(this IServiceCollection service, ObjectStoreOption option)
    {
        service.AddSingleton(option);
        service.AddSingleton<ObjectStoreFactory>();
        service.AddSingleton<ObjectStoreApi>();
        service.AddSingleton<ObjectStoreEndpoint>();

        return service;
    }

    public static void MapObjectStore(this IEndpointRouteBuilder app)
    {
        ObjectStoreEndpoint endpoint = app.ServiceProvider.GetRequiredService<ObjectStoreEndpoint>();

        var group = app.MapGroup("/data");
        group.MapGet("/{objectId}", async (string objectId, CancellationToken token) => await endpoint.Read(objectId, new ScopeContext(token)))
            .WithName("Read")
            .WithOpenApi();

        group.MapPost("/", async (Document document, CancellationToken token) => await endpoint.Write(document, new ScopeContext(token)))
            .WithName("Write")
            .WithOpenApi();

        group.MapDelete("/{objectId}", async (string objectId, CancellationToken token) => await endpoint.Delete(objectId, new ScopeContext(token)))
            .WithName("Delete")
            .WithOpenApi();
    }
}
