using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Actor
{
    public class ActorHost : IActorHost, IDisposable
    {
        private readonly object _lock = new object();
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<ActorHost> _logger;
        private readonly ActorRegistry _registry;
        private readonly ActorCollection _actorCollection;

        public ActorHost(ILoggerFactory loggerFactory)
            : this(1000, loggerFactory)
        {
        }

        public ActorHost(int capacity, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<ActorHost>();

            _registry = new ActorRegistry(_loggerFactory.CreateLogger<ActorRegistry>());
            _actorCollection = new ActorCollection(capacity, _loggerFactory.CreateLogger<ActorCollection>());
        }

        public TimeSpan ActorCallTimeout { get; set; } = TimeSpan.FromMinutes(5);

        public ActorHost Register(Type type, Func<IActor> createImplementation)
        {
            _registry.Register(type, createImplementation);
            return this;
        }

        public ActorHost Register<T>(Func<IActor> createImplementation)
        {
            _registry.Register(typeof(T), createImplementation);
            return this;
        }

        public ActorHost Unregister(Type type)
        {
            _registry.Unregister(type);
            return this;
        }

        /// <summary>
        /// Get actor, if already exist, will return or create new one
        /// </summary>
        /// <typeparam name="T">actor type</typeparam>
        /// <param name="actorKey">actor key</param>
        /// <returns>actor instance</returns>
        public T GetActor<T>(ActorKey actorKey) where T : IActor
        {
            actorKey.VerifyNotNull(nameof(actorKey));

            Type actoryType = typeof(T);

            lock (_lock)
            {
                if (_actorCollection.TryGetValue(actoryType, actorKey, out ActorInstance actorInstance))
                {
                    return (T)actorInstance.GetInstance<T>();
                }

                // Create actor object
                IActorBase actorBase = _registry.Create<T>(actorKey, this);

                // Create proxy
                T actorInterface = ActorProxy<T>.Create(actorBase, this);
                actorInstance = new ActorInstance(typeof(T), actorKey, actorBase, actorInterface);

                // Add to actor collection
                _actorCollection.Set(actorInstance);

                // Create proxy for interface
                return actorInstance.GetInstance<T>();
            }
        }

        /// <summary>
        /// Deactivate actor
        /// </summary>
        /// <typeparam name="T">actor interface</typeparam>
        /// <param name="context">context</param>
        /// <param name="actorKey">actor key</param>
        /// <returns>true if deactivated, false if not found</returns>
        public async Task<bool> Deactivate<T>(ActorKey actorKey)
        {
            actorKey.VerifyNotNull(nameof(actorKey));

            if (!_actorCollection.TryRemove(typeof(T), actorKey, out ActorInstance registration)) return false;

            _logger.LogTrace($"Deactivating and removing actor {actorKey}");

            try
            {
                await registration.Instance.Deactivate();
            }
            finally
            {
                registration.Instance.Dispose();
            }

            return true;
        }

        /// <summary>
        /// Deactivate all actors
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>task</returns>
        public async Task DeactivateAll()
        {
            IReadOnlyList<ActorInstance> actorInstances = _actorCollection.Clear();

            await actorInstances
                .Select(x => x.Instance.Deactivate())
                .WhenAll();
        }

        /// <summary>
        /// Dispose of all resources, actors are deactivated, DI container is disposed
        /// </summary>
        public void Dispose()
        {
            DeactivateAll()
                .GetAwaiter()
                .GetResult();
        }
    }
}
