using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Toolbox.Tools;

namespace Toolbox.Actor.Host
{
    /// <summary>
    /// Actor instance registration information
    /// </summary>
    public class ActorInstance
    {
        private IActorBase? _instance;
        private IActor? _actorProxy;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="actorType">actor type</param>
        /// <param name="actorKey">actor key</param>
        /// <param name="instance">instance of the actor class</param>
        /// <param name="actorProxy">actor proxy</param>
        public ActorInstance(Type actorType, ActorKey actorKey, IActorBase instance, IActor actorProxy)
        {
            actorType.NotNull();
            actorKey.NotNull();
            instance.NotNull();
            actorProxy.NotNull();

            ActorType = actorType;
            ActorKey = actorKey;
            _instance = instance;
            _actorProxy = actorProxy;
        }

        /// <summary>
        /// Actor key
        /// </summary>
        public ActorKey ActorKey { get; }

        /// <summary>
        /// Actor instance
        /// </summary>
        public IActorBase Instance => _instance ?? throw new InvalidOperationException("Actor Registration is Disposed");

        /// <summary>
        /// Type of actor
        /// </summary>
        public Type ActorType { get; }

        /// <summary>
        /// Proxy to actor
        /// </summary>
        public IActor ActorProxy => _actorProxy ?? throw new InvalidOperationException("Actor Registration is disposed");

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Interlocked.Exchange(ref _actorProxy, null);

            IActorBase? current = Interlocked.Exchange(ref _instance, null);
            current?.Dispose();
        }

        /// <summary>
        /// Get instance
        /// </summary>
        /// <typeparam name="T">actor type</typeparam>
        /// <returns>instance of actor</returns>
        public T GetInstance<T>() where T : IActor => (T)ActorProxy;
    }
}
