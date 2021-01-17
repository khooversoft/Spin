using System;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Actor
{
    /// <summary>
    /// Actor key, a GUID is created from the string vector
    /// </summary>
    public record ActorKey
    {
        /// <summary>
        /// Construct actor key from vector
        /// </summary>
        /// <param name="vectorKey"></param>
        public ActorKey(string vectorKey)
        {
            vectorKey.VerifyNotNull(nameof(vectorKey));

            Value = vectorKey.ToLowerInvariant();
            Key = Value.ToGuid();
        }

        public static ActorKey Default { get; } = new ActorKey("default");

        /// <summary>
        /// Actor key (hash from vector key)
        /// </summary>
        public Guid Key { get; }

        /// <summary>
        /// Key value
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Explicit convert actor key to string
        /// </summary>
        /// <param name="actorKey"></param>
        public static explicit operator string(ActorKey actorKey)
        {
            actorKey.VerifyNotNull(nameof(actorKey));

            return actorKey.Value;
        }

        public static explicit operator ActorKey(string value) => new ActorKey(value);
    }
}