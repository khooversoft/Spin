using System;
using System.Threading.Tasks;

namespace Toolbox.Actor
{
    public interface IActorBase : IDisposable
    {
        ActorKey ActorKey { get; }

        bool Active { get; }

        Task Activate();

        Task Deactivate();
    }
}
