using System;
using System.Collections.Generic;
using Toolbox.Tools;

namespace Toolbox.Actor
{
    /// <summary>
    /// Actor key, a GUID is created from the string vector
    /// </summary>
    public class ActorKey
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

        public override bool Equals(object? obj)
        {
            return obj is ActorKey key &&
                   Key.Equals(key.Key);
        }

        public override int GetHashCode() => HashCode.Combine(Key);

        public override string ToString() => $"Key={Key}, Vector key={Value}";

        public static bool operator ==(ActorKey? left, ActorKey? right) => EqualityComparer<ActorKey>.Default.Equals(left!, right!);

        public static bool operator !=(ActorKey? left, ActorKey? right) => !(left == right);

        /// <summary>
        /// Implicit convert actor key to string
        /// </summary>
        /// <param name="actorKey"></param>
        public static explicit operator string(ActorKey actorKey)
        {
            actorKey.VerifyNotNull(nameof(actorKey));

            return actorKey.Value;
        }

        public static explicit operator ActorKey(string value) => new ActorKey(value.VerifyNotEmpty(nameof(value)));
    }
}
