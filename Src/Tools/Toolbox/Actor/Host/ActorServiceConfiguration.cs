using Microsoft.Extensions.DependencyInjection;
using System;
using Toolbox.Tools;

namespace Toolbox.Actor.Host;

public interface IActorServiceConfiguration
{
    IActorServiceConfiguration Register(Type type, Func<IActor> createImplementation);
    IActorServiceConfiguration Register<T, TImpl>() where T : IActor where TImpl : IActor;
    IActorServiceConfiguration Register<T>(Func<IActor> createImplementation);
    IActorServiceConfiguration Register<T>() where T : IActor;
}


internal class ActorServiceConfiguration : IActorServiceConfiguration
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IActorService _actorHost;

    public ActorServiceConfiguration(IActorService actorHost, IServiceProvider serviceProvider)
    {
        _actorHost = actorHost.NotNull();
        _serviceProvider = serviceProvider.NotNull();
    }

    public IActorServiceConfiguration Register(Type type, Func<IActor> createImplementation)
    {
        _actorHost.Register(type, createImplementation);
        return this;
    }

    public IActorServiceConfiguration Register<T>(Func<IActor> createImplementation)
    {
        _actorHost.Register<T>(createImplementation);
        return this;
    }

    public IActorServiceConfiguration Register<T>() where T : IActor
    {
        _actorHost.Register<T>(() => _serviceProvider.GetRequiredService<T>());
        return this;
    }

    public IActorServiceConfiguration Register<T, TImpl>() where T : IActor where TImpl : IActor
    {
        _actorHost.Register<T>(() => _serviceProvider.GetRequiredService<TImpl>());
        return this;
    }
}
