using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Actor.Host;

/// <summary>
/// Registry for actor types
/// </summary>
public class ActorRegistry
{
    private readonly ConcurrentDictionary<Type, ActorRegistration> _typeRegistry = new ConcurrentDictionary<Type, ActorRegistration>();
    private readonly ILogger<ActorRegistry> _logger;

    public ActorRegistry(ILogger<ActorRegistry> logger)
    {
        _logger = logger.NotNull();
    }

    public void Register(Type type, Func<IActor> createImplementation)
    {
        type
            .NotNull()
            .IsInterface.Assert(x => x == true, $"{type} must be an interface");

        createImplementation.NotNull();

        _typeRegistry[type] = new ActorRegistration(type, createImplementation);
        _logger.LogTrace("Type={type} is registered", type);
    }

    public bool Unregister(Type type) => _typeRegistry
        .Remove(type.NotNull(), out var _)
        .Action(x => _logger.LogTrace("Type={type} is un-registered", type));

    /// <summary>
    /// Create actor from either lambda or activator
    /// </summary>
    /// <typeparam name="T">actor interface</typeparam>
    /// <param name="context">context</param>
    /// <param name="actorKey">actor key</param>
    /// <param name="actorHost">actor manager</param>
    /// <returns>instance of actor implementation</returns>
    public IActorBase Create<T>(ActorKey actorKey, ActorService actorHost) where T : IActor
    {
        actorKey.NotNull();
        actorHost.NotNull();

        Type actorType = typeof(T)
            .Assert(x => x.IsInterface, $"{typeof(T)} must be an interface");

        ActorRegistration typeRegistration = GetTypeRegistration(actorType);

        IActor actorObject = typeRegistration.CreateImplementation();

        // Set actor key and manager
        ActorBase? actorBase = actorObject as ActorBase;
        if (actorBase == null)
        {
            var ex = new InvalidOperationException($"Created actor type {actorObject.GetType()} does not derive from ActorBase");
            _logger.LogError(ex, "Not valid type");
            throw ex;
        }

        actorBase.ActorKey = actorKey;
        actorBase.ActorHost = actorHost;
        actorBase.ActorType = actorType;

        _logger.LogTrace("Actor interface={interface}, type={actorType} is created", typeof(T).Name, actorObject.GetType().Name);
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
