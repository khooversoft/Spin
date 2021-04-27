using Toolbox.Azure.DataLake.Model;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace ArtifactStore.Application
{
    public static class OptionExtensions
    {
        public static void Verify(this Option option)
        {
            option.VerifyNotNull(nameof(option));

            option.ApiKey.VerifyNotEmpty($"{nameof(option.ApiKey)} is required");
            option.Stores.ForEach(x => x.Verify());
        }
    }
}