using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Toolbox.Extensions;
using Toolbox.Services;
using Toolbox.Tools;

namespace Toolbox.Application
{
    public static class OptionExtensions
    {
        public static void LogConfigurations<T>(this ILogger logger, T option, ISecretFilter? secretFilter = null, string? title = null) where T : class
        {
            const int maxWidth = 80;

            string line = option.GetConfigurationValues()
                .Select(x => $"{x.Key}={x.Value}")
                .Prepend(new string('=', maxWidth))
                .Prepend(title ?? "Current configurations")
                .Aggregate(string.Empty, (a, x) => a += (secretFilter?.FilterSecrets(x) ?? x) + Environment.NewLine);

            logger.LogInformation(line);
        }

        public static Stream GetResourceStream(this RunEnvironment runEnvironment, Type type, string baseResourceId)
        {
            const string configSufix = "-config.json";

            if (!baseResourceId.EndsWith(".")) baseResourceId += ".";

            string resourceId = baseResourceId + runEnvironment switch
            {
                RunEnvironment.Unknown => "unknown",
                RunEnvironment.Local => "local",
                RunEnvironment.Dev => "dev",
                RunEnvironment.PreProd => "preProd",
                RunEnvironment.Prod => "prod",

                _ => throw new ArgumentException($"Unknown RunEnvironment=(int){(int)runEnvironment}"),
            } + configSufix;

            return Assembly.GetAssembly(type)!
                .GetManifestResourceStream(resourceId)
                .NotNull(name: $"Resource {resourceId} not found in assembly");
        }
    }
}
