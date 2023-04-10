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
    //public static IServiceCollection AddActor(this IServiceCollection collection)
    //{
    //    collection.NotNull();

    //    collection.AddSingleton<IActorService, ActorService>();
    //    return collection;
    //}

    public static IServiceCollection AddActor(this IServiceCollection collection, Action<IActorServiceConfiguration> actorHostConfiguration)
    {
        collection.NotNull();
        actorHostConfiguration.NotNull();

        collection.AddSingleton<IActorService>(service =>
        {
            var host = new ActorService(service.GetRequiredService<ILoggerFactory>());
            actorHostConfiguration(new ActorServiceConfiguration(host, service));
            return host;
        });

        return collection;
    }

    public static T GetActor<T>(this IServiceProvider provider, ActorKey actorKey) where T : IActor
    {
        provider.NotNull();
        actorKey.NotNull();

        var host = provider.GetRequiredService<IActorService>();
        return host.GetActor<T>(actorKey);
    }
}
