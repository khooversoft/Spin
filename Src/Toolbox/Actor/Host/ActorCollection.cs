using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using Toolbox.Actor.Tools;
using Toolbox.Tools;

namespace Toolbox.Actor
{
    public class ActorCollection
    {
        private readonly LruCache<(Type Type, Guid Key), ActorInstance> _actorCache;
        private readonly ActionBlock<ActorInstance> _actorRemove;
        private readonly object _lock = new object();
        private readonly ILogger<ActorCollection> _logger;

        public ActorCollection(int capacity, ILogger<ActorCollection> logger)
        {
            capacity.VerifyAssert(x => x > 0, "Capacity must be greater then 0");
            logger.VerifyNotNull(nameof(logger));

            _actorRemove = new ActionBlock<ActorInstance>(async x => await x.Instance.Deactivate());
            _logger = logger;
            _actorCache = new LruCache<(Type, Guid), ActorInstance>(capacity);
            _actorCache.CacheItemRemoved += x => _actorRemove.Post(x.Value);
        }

        /// <summary>
        /// Clear all actors from the system.  Each active actor will be deactivated
        /// </summary>
        /// <param name="context">context</param>
        /// <returns></returns>
        public IReadOnlyList<ActorInstance> Clear()
        {
            _logger.LogTrace("Clearing actor container");
            List<ActorInstance> list;

            lock (_lock)
            {
                list = new List<ActorInstance>(_actorCache.GetValues());
                _actorCache.Clear();
                return list;
            }
        }

        /// <summary>
        /// Set actor (add or replace)
        /// </summary>
        /// <param name="registration">actor registration</param>
        /// <returns>task</returns>
        public void Set(ActorInstance registration)
        {
            registration.VerifyNotNull(nameof(registration));

            _logger.LogTrace($"Setting actor {registration.ActorKey}");
            ActorInstance? currentActorRegistration = null;

            lock (_lock)
            {
                if (!_actorCache.TryRemove((registration.ActorType, registration.ActorKey.Key), out currentActorRegistration))
                {
                    currentActorRegistration = null;
                }

                _actorCache.Set((registration.ActorType, registration.ActorKey.Key), registration);
            }

            // Dispose of the old actor
            if (currentActorRegistration != null) _actorRemove.Post(currentActorRegistration);

            registration.Instance!.Activate().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Lookup actor
        /// </summary>
        /// <param name="actorType">actor type</param>
        /// <param name="actorKey">actor key</param>
        /// <param name="actorInstance">return instance</param>
        /// <returns>true or false</returns>
        public bool TryGetValue(Type actorType, ActorKey actorKey, out ActorInstance actorInstance)
        {
            actorType.VerifyNotNull(nameof(actorType));
            actorKey.VerifyNotNull(nameof(actorKey));

            lock (_lock)
            {
                return _actorCache.TryGetValue((actorType, actorKey.Key), out actorInstance);
            }
        }

        /// <summary>
        /// Remove actor from container
        /// </summary>
        /// <param name="actorType">actor type</param>
        /// <param name="actorKey">actor key</param>
        /// <returns>actor registration or null if not exist</returns>
        public bool TryRemove(Type actorType, ActorKey actorKey, out ActorInstance actorInstance)
        {
            actorKey.VerifyNotNull(nameof(actorKey));
            _logger.LogTrace($"Removing actor {actorKey}");

            lock (_lock)
            {
                if (!_actorCache.TryRemove((actorType, actorKey.Key), out actorInstance)) return false;
            }

            actorInstance.Instance
                .Deactivate().GetAwaiter().GetResult();

            return true;
        }
    }
}
