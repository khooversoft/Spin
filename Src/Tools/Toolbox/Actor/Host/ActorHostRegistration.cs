using Microsoft.Extensions.DependencyInjection;
using System;
using Toolbox.Tools;

namespace Toolbox.Actor.Host;

public interface IActorHostRegistration
{
    IActorHostRegistration Register(Type type, Func<IActor> createImplementation);
    IActorHostRegistration Register<T, TImpl>() where T : IActor where TImpl : IActor;
    IActorHostRegistration Register<T>(Func<IActor> createImplementation);
}


internal class ActorHostRegistration : IActorHostRegistration
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IActorHost _actorHost;

    public ActorHostRegistration(IActorHost actorHost, IServiceProvider serviceProvider)
    {
        _actorHost = actorHost.NotNull();
        _serviceProvider = serviceProvider.NotNull();
    }

    public IActorHostRegistration Register(Type type, Func<IActor> createImplementation)
    {
        _actorHost.Register(type, createImplementation);
        return this;
    }

    public IActorHostRegistration Register<T, TImpl>() where T : IActor where TImpl : IActor
    {
        _actorHost.Register<T>(() => _serviceProvider.GetRequiredService<TImpl>());
        return this;
    }

    public IActorHostRegistration Register<T>(Func<IActor> createImplementation)
    {
        _actorHost.Register<T>(createImplementation);
        return this;
    }
}
