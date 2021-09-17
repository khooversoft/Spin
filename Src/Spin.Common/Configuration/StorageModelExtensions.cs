using Toolbox.Tools;

namespace Spin.Common.Configuration
{
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
