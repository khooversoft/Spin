using System;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Actor.Host;

namespace Toolbox.Actor
{
    /// <summary>
    /// Base class for actors
    /// </summary>
    public abstract class ActorBase : IActorBase
    {
        private int _running = 0;

        private const int _stateStopped = 0;
        private const int _stateRunning = 1;

        /// <summary>
        /// Get actor key
        /// </summary>
        public ActorKey ActorKey { get; internal set; } = ActorKey.Default;

        /// <summary>
        /// Get actor manager
        /// </summary>
        public IActorHost ActorHost { get; internal set; } = null!;

        /// <summary>
        /// Actor type, to deactivate
        /// </summary>
        public Type ActorType { get; internal set; } = null!;

        /// <summary>
        /// Actor is active (running)
        /// </summary>
        public bool Active => _running == 1;

        /// <summary>
        /// Activate actor
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>task</returns>
        public async Task Activate()
        {
            int currentValue = Interlocked.CompareExchange(ref _running, _stateRunning, _stateStopped);
            if (currentValue != 0)
            {
                return;
            }

            await OnActivate().ConfigureAwait(false);
        }

        /// <summary>
        /// Deactivate actor
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>task</returns>
        public async Task Deactivate()
        {
            int currentValue = Interlocked.CompareExchange(ref _running, _stateStopped, _stateRunning);
            if (currentValue != 1)
            {
                return;
            }

            await OnDeactivate().ConfigureAwait(false);
        }

        /// <summary>
        /// Dispose, virtual
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Event for on activate actor
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>task</returns>
        protected virtual Task OnActivate() => Task.CompletedTask;

        /// <summary>
        /// Event for on deactivate actor
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>task</returns>
        protected virtual Task OnDeactivate() => Task.CompletedTask;
    }
}
