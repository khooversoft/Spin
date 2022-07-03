using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Actor.Host;

public static class Extensions
{
    public static IServiceCollection AddActorHost(this IServiceCollection collection)
    {
        collection.NotNull();

        collection.AddSingleton<IActorHost, ActorHost>();
        return collection;
    }

    public static IServiceCollection AddActorHost(this IServiceCollection collection, Action<IActorHostRegistration> actorHostConfiguration)
    {
        collection.NotNull();
        actorHostConfiguration.NotNull();

        collection.AddSingleton<IActorHost>(service =>
        {
            var host = new ActorHost(service.GetRequiredService<ILoggerFactory>());
            actorHostConfiguration(new ActorHostRegistration(host, service));
            return host;
        });

        return collection;
    }

    public static T GetActor<T>(this IServiceProvider provider, ActorKey actorKey) where T : IActor
    {
        provider.NotNull();
        actorKey.NotNull();

        var host = provider.GetRequiredService<IActorHost>();
        return host.GetActor<T>(actorKey);
    }
}
