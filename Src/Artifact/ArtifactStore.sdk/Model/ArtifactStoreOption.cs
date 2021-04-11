using Spin.Common.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace ArtifactStore.sdk.Model
{
    public record ArtifactStoreOption
    {
        public string ApiKey { get; init; } = null!;

        public string Url { get; init; } = null!;
    }

    public static class ArtifactStoreOptionExtensions
    {
        public static void Verify(this ArtifactStoreOption artifactStoreOption)
        {
            artifactStoreOption.VerifyNotNull(nameof(artifactStoreOption));
            artifactStoreOption.ApiKey.VerifyNotEmpty(nameof(artifactStoreOption.ApiKey));
            artifactStoreOption.Url.VerifyNotEmpty(nameof(artifactStoreOption.Url));
        }

        public static (string Key, string Value) GetApiHeader(this ArtifactStoreOption artifactStoreOption) => (Constants.ApiKeyName, artifactStoreOption.ApiKey);
    }
}
