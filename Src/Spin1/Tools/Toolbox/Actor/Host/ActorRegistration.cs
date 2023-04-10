using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Tools;

namespace Toolbox.Actor.Host
{
    /// <summary>
    /// Actor registration for lambda activator
    /// </summary>
    public class ActorRegistration
    {
        public ActorRegistration(Type interfaceType, Func<IActor> createImplementation)
        {
            interfaceType.NotNull();
            createImplementation.NotNull();

            InterfaceType = interfaceType;
            CreateImplementation = createImplementation;
        }

        /// <summary>
        /// Interface type
        /// </summary>
        public Type InterfaceType { get; }

        /// <summary>
        /// Create implementation by lambda
        /// </summary>
        public Func<IActor> CreateImplementation { get; }
    }
}
