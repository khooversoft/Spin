using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ObjectStore.sdk.Connectors;
using Toolbox.Data;
using Toolbox.Types;

namespace ObjectStore.sdk.Application;

public static class Startup
{
    public static IServiceCollection AddObjectStore(this IServiceCollection service, ObjectStoreOption option)
    {
        service.AddSingleton(option);
        service.AddSingleton<ObjectStoreFactory>();
        service.AddSingleton<ObjectStoreConnector>();
        service.AddSingleton<ObjectStoreEndpoint>();

        return service;
    }

    public static void MapObjectStore(this IEndpointRouteBuilder app)
    {
        ObjectStoreEndpoint endpoint = app.ServiceProvider.GetRequiredService<ObjectStoreEndpoint>();

        var group = app.MapGroup("/data");

        group.MapGet("/{objectId}", async (string objectId, CancellationToken token) => await endpoint.Read(objectId, new ScopeContext(endpoint.Logger, token)))
            .WithName("Read")
            .WithOpenApi();

        group.MapPost("/", async (Document document, CancellationToken token) => await endpoint.Write(document, new ScopeContext(endpoint.Logger, token)))
            .WithName("Write")
            .WithOpenApi();

        group.MapDelete("/{objectId}", async (string objectId, CancellationToken token) => await endpoint.Delete(objectId, new ScopeContext(endpoint.Logger, token)))
            .WithName("Delete")
            .WithOpenApi();

        group.MapPost("/search", async (QueryParameter query, CancellationToken token) => await endpoint.Search(query, new ScopeContext(endpoint.Logger, token)))
            .WithName("Search")
            .WithOpenApi();
    }
}
