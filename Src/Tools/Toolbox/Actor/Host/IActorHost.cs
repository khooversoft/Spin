using System;
using System.Threading.Tasks;

namespace Toolbox.Actor.Host;

public interface IActorHost : IDisposable
{
    TimeSpan ActorCallTimeout { get; set; }

    Task<bool> Deactivate<T>(ActorKey actorKey);
    Task DeactivateAll();
    T GetActor<T>(ActorKey actorKey) where T : IActor;
    IActorHost Register(Type type, Func<IActor> createImplementation);
    IActorHost Register<T>(Func<IActor> createImplementation);
    IActorHost Unregister(Type type);
}
