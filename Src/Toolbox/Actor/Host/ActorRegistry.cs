using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Tools;

namespace Toolbox.Actor.Host
{
    /// <summary>
    /// Registry for actor types
    /// </summary>
    public class ActorRegistry
    {
        private readonly Dictionary<Type, ActorRegistration> _typeRegistry = new Dictionary<Type, ActorRegistration>();
        private readonly ILogger<ActorRegistry> _logger;

        public ActorRegistry(ILogger<ActorRegistry> logger)
        {
            _logger = logger;
        }

        public ActorRegistration? this[Type type] => _typeRegistry.TryGetValue(type, out ActorRegistration? value) ? value : null;

        public void Register(Type type, Func<IActor> createImplementation)
        {
            type
                .VerifyNotNull(nameof(type))
                .IsInterface.VerifyAssert(x => x == true, $"{type} must be an interface");

            createImplementation.VerifyNotNull(nameof(createImplementation));

            _typeRegistry[type] = new ActorRegistration(type, createImplementation);
        }

        public bool Unregister(Type type) => _typeRegistry.Remove(type.VerifyNotNull(nameof(type)));

        /// <summary>
        /// Create actor from either lambda or activator
        /// </summary>
        /// <typeparam name="T">actor interface</typeparam>
        /// <param name="context">context</param>
        /// <param name="actorKey">actor key</param>
        /// <param name="actorHost">actor manager</param>
        /// <returns>instance of actor implementation</returns>
        public IActorBase Create<T>(ActorKey actorKey, ActorHost actorHost) where T : IActor
        {
            actorKey.VerifyNotNull(nameof(actorKey));
            actorHost.VerifyNotNull(nameof(actorHost));

            Type actorType = typeof(T)
                .VerifyAssert(x => x.IsInterface, $"{typeof(T)} must be an interface");

            ActorRegistration typeRegistration = GetTypeRegistration(actorType);

            IActor actorObject = typeRegistration.CreateImplementation();

            // Set actor key and manager
            ActorBase? actorBase = actorObject as ActorBase;
            if (actorBase == null)
            {
                string failureMsg = $"Created actor type {actorObject.GetType()} does not derive from ActorBase";
                _logger.LogError(failureMsg);
                throw new InvalidOperationException(failureMsg);
            }

            actorBase.ActorKey = actorKey;
            actorBase.ActorHost = actorHost;
            actorBase.ActorType = actorType;

            return (IActorBase)actorObject;
        }

        private ActorRegistration GetTypeRegistration(Type actorType)
        {
            if (_typeRegistry.TryGetValue(actorType, out ActorRegistration? typeRegistration))
            {
                return typeRegistration;
            }

            var ex = new KeyNotFoundException($"Registration for {actorType.FullName} was not found");
            _logger.LogError(ex, "create failure");
            throw ex;
        }
    }
}
