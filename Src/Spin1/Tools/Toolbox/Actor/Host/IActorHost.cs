using System;
using System.Threading.Tasks;

namespace Toolbox.Actor.Host;

public interface IActorService : IDisposable
{
    TimeSpan ActorCallTimeout { get; set; }

    Task<bool> Deactivate<T>(ActorKey actorKey);
    Task DeactivateAll();
    T GetActor<T>(ActorKey actorKey) where T : IActor;
    IActorService Register(Type type, Func<IActor> createImplementation);
    IActorService Register<T>(Func<IActor> createImplementation);
    IActorService Unregister(Type type);
}
