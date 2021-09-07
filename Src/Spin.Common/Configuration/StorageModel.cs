using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Spin.Common.Configuration
{
    public class StorageModel
    {
        public string AccountName { get; init; } = null!;

        public string ContainerName { get; init; } = null!;

        public override bool Equals(object? obj) => obj is StorageModel model &&
                   AccountName.Equals(model.AccountName, StringComparison.OrdinalIgnoreCase) &&
                   ContainerName.Equals(model.ContainerName, StringComparison.OrdinalIgnoreCase);

        public override int GetHashCode() => HashCode.Combine(AccountName, ContainerName);

        public override string? ToString() => $"{{ AccountName={AccountName}, ContainerName={ContainerName} }}";

        public static bool operator ==(StorageModel? left, StorageModel? right) => EqualityComparer<StorageModel>.Default.Equals(left, right);

        public static bool operator !=(StorageModel? left, StorageModel? right) => !(left == right);
    }


    public static class StorageModelExtensions
    {
        public static void Verify(this StorageModel storageModel)
        {
            storageModel.VerifyNotNull(nameof(storageModel));

            storageModel.AccountName.VerifyNotEmpty($"{nameof(storageModel.AccountName)} is required");
            storageModel.ContainerName.VerifyNotEmpty($"{nameof(storageModel.ContainerName)} is required");
        }
    }
}
