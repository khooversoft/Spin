using System;
using System.Collections.Generic;

namespace Spin.Common.Configuration
{
    public class QueueModel
    {
        public string Namespace { get; init; } = null!;

        public string Name { get; init; } = null!;

        public override bool Equals(object? obj) => obj is QueueModel model &&
                Namespace.Equals(model.Namespace, StringComparison.OrdinalIgnoreCase) &&
                Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase);

        public override int GetHashCode() => HashCode.Combine(Namespace, Name);

        public override string? ToString() => $"{{ Namespace={Namespace}, Name={Name} }}";

        public static bool operator ==(QueueModel? left, QueueModel? right) => EqualityComparer<QueueModel>.Default.Equals(left, right);

        public static bool operator !=(QueueModel? left, QueueModel? right) => !(left == right);
    }
}