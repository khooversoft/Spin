using System.Text;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace ArtifactCmd.Application
{
    internal record ConfigOption
    {
        public RunEnvironment? Environment { get; init; }

        public string ApiKey { get; init; } = null!;

        public string ArtifactUrl { get; init; } = null!;

        public string Namespace { get; set; } = null!;
    }

    internal static class ConfigOptionExtensions
    {
        public static ConfigOption Verify(this ConfigOption configOption)
        {
            configOption.VerifyNotNull(nameof(configOption));
            configOption.ApiKey.VerifyNotEmpty($"{nameof(configOption.ApiKey)} is required");
            configOption.ArtifactUrl.VerifyNotEmpty($"{nameof(configOption.ArtifactUrl)} is required");
            configOption.Namespace.VerifyNotEmpty($"{nameof(configOption.Namespace)} is required");

            return configOption;
        }
    }
}
