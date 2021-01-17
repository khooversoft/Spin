using System;
using System.Threading.Tasks;

namespace Toolbox.Actor.Host
{
    public interface IActorHost : IDisposable
    {
        TimeSpan ActorCallTimeout { get; set; }

        Task<bool> Deactivate<T>(ActorKey actorKey);
        Task DeactivateAll();
        T GetActor<T>(ActorKey actorKey) where T : IActor;
        ActorHost Register(Type type, Func<IActor> createImplementation);
        ActorHost Register<T>(Func<IActor> createImplementation);
        ActorHost Unregister(Type type);
    }
}