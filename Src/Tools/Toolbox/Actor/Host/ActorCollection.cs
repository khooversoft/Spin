using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Toolbox.Actor;
using Toolbox.Actor.Tools;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Actor.Host
{
    public class ActorCollection
    {
        private readonly LruCache<ActorTypeKey, ActorInstance> _actorCache;
        private readonly ActionBlock<ActorInstance> _actorRemove;
        private readonly object _lock = new object();
        private readonly ILogger<ActorCollection> _logger;

        public ActorCollection(int capacity, ILogger<ActorCollection> logger)
        {
            capacity.VerifyAssert(x => x > 0, "Capacity must be greater then 0");
            logger.VerifyNotNull(nameof(logger));

            _actorRemove = new ActionBlock<ActorInstance>(async x => await x.Instance.Deactivate());
            _logger = logger;
            _actorCache = new LruCache<ActorTypeKey, ActorInstance>(capacity);
            _actorCache.CacheItemRemoved += x => _actorRemove.Post(x.Value);
        }

        /// <summary>
        /// Clear all actors from the system.  Each active actor will be deactivated
        /// </summary>
        /// <param name="context">context</param>
        /// <returns></returns>
        public IReadOnlyList<ActorInstance> Clear()
        {
            _logger.LogTrace($"{nameof(Clear)}: Clearing actor container");
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

            _logger.LogTrace($"{nameof(Set)}: Setting actor {registration.ActorKey}");
            ActorInstance? currentActorRegistration = null;

            lock (_lock)
            {
                if (!_actorCache.TryRemove(new ActorTypeKey(registration.ActorType, registration.ActorKey), out currentActorRegistration))
                {
                    _logger.LogTrace($"{nameof(Set)}: No current instance of {registration.ActorKey}");
                    currentActorRegistration = null;
                }

                _actorCache.Set(new ActorTypeKey(registration.ActorType, registration.ActorKey), registration);
                _logger.LogTrace($"{nameof(Set)}: Adding new registration to cache {registration.ActorKey}, _actorCache.Count={_actorCache.Count}");
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
                bool status = _actorCache.TryGetValue(new ActorTypeKey(actorType, actorKey), out actorInstance);
                _logger.LogTrace($"{nameof(TryGetValue)}: status={status}, actorKey={actorKey}");

                return status;
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

            lock (_lock)
            {
                bool status = _actorCache.TryRemove(new ActorTypeKey(actorType, actorKey), out actorInstance);
                _logger.LogTrace($"{nameof(TryRemove)}: Removing actor {actorKey}, status={status}");

                if (!status) return status;
            }

            actorInstance.Instance
                .Deactivate().GetAwaiter().GetResult();

            return true;
        }

        private record ActorTypeKey
        {
            public ActorTypeKey(Type actorType, ActorKey actorKey)
            {
                ActorTypeName = actorType.FullName.VerifyNotEmpty(nameof(actorType.FullName));
                ActorKeyGuid = actorKey.Key;
            }

            public string ActorTypeName { get; }

            public Guid ActorKeyGuid { get; }
        }
    }
}
