using System;
using System.Collections.Generic;
using Toolbox.Tools;

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


    public static class QueueModelExtensions
    {
        public static void Verify(this QueueModel queueModel)
        {
            queueModel.VerifyNotNull(nameof(queueModel));

            queueModel.Namespace.VerifyNotEmpty($"{nameof(queueModel.Namespace)} is required");
            queueModel.Name.VerifyNotEmpty($"{nameof(queueModel.Name)} is required");
        }
    }
}