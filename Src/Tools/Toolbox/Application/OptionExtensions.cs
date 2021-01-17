using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Application
{
    public static class OptionExtensions
    {
        public static void LogConfigurations<T>(this ILogger logger, T option) where T : class
        {
            const int maxWidth = 80;

            string line = option.GetConfigValues()
                .Prepend(new string('=', maxWidth))
                .Prepend("Current configurations")
                .Aggregate(string.Empty, (a, x) => a += x + Environment.NewLine);

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
                .VerifyNotNull($"Resource {resourceId} not found in assembly");
        }
    }
}
