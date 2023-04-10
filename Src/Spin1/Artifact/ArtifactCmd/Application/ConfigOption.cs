using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
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
    }

    internal static class ConfigOptionExtensions
    {
        public static bool IsValid(this ConfigOption configOption, ILogger logger)
        {
            return new (bool Status, string Message)[]
            {
                (!configOption.ApiKey.IsEmpty(), $"{nameof(configOption.ApiKey)} is required"),
                (!configOption.ArtifactUrl.IsEmpty(), $"{nameof(configOption.ArtifactUrl)} is required"),
            }
            .All(x =>
            {
                if (!x.Status) logger.LogError(x.Message);
                return x.Status;
            });
        }
    }
}
