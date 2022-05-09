using System;
using System.Reflection;
using System.Threading;
using Toolbox.Actor.Host;
using Toolbox.Tools;

namespace Toolbox.Actor
{
    /// <summary>
    /// Actor proxy built by RealProxy class in .NET.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    //[DebuggerStepThrough]
    public class ActorProxy<T> : DispatchProxy where T : IActor
    {
        private readonly SemaphoreSlim _lockSemaphore = new SemaphoreSlim(1, 1);
        private IActorBase? _instance;
        private ActorHost? _actorHost;

        public ActorProxy()
        {
        }

        /// <summary>
        /// Create transparent proxy for instance of actor class
        /// </summary>
        /// <param name="context">work context</param>
        /// <param name="instance">instance of actor class</param>
        /// <param name="manager">actor manager</param>
        /// <returns>proxy</returns>
        public static T Create(IActorBase instance, ActorHost actorHost)
        {
            instance.NotNull(nameof(instance));
            actorHost.NotNull(nameof(actorHost));

            object proxyObject = Create<T, ActorProxy<T>>();

            ActorProxy<T> proxy = (ActorProxy<T>)proxyObject;
            proxy._instance = instance;
            proxy._actorHost = actorHost;

            return (T)proxyObject;
        }

        /// <summary>
        /// Invoke method, called by the dispatch proxy
        /// </summary>
        /// <param name="targetMethod">method info</param>
        /// <param name="args">args</param>
        /// <returns>return value</returns>
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            targetMethod.NotNull(nameof(targetMethod));

            try
            {
                _lockSemaphore.Wait(_actorHost?.ActorCallTimeout ?? TimeSpan.FromMinutes(5));
                return targetMethod.Invoke(_instance, args);
            }
            finally
            {
                _lockSemaphore.Release();
            }
        }
    }
}
