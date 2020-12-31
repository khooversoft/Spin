using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Tools;

namespace ArtifactStore.Application
{
    public class OptionBuilder : OptionBuilder<Option>
    {
        public OptionBuilder() =>
            SetFinalize(FinalizeOption)
            .SetConfigStream(GetResourceStream);

        private Option FinalizeOption(Option option, RunEnvironment runEnvironment)
        {
            option.Verify();

            option = option with
            {
                RunEnvironment = runEnvironment,

                Store = new DataLakeNamespaceOption
                {
                    Namespaces = option.Store.Namespaces.Values.ToDictionary(x => x.Namespace, x => x, StringComparer.OrdinalIgnoreCase),
                }
            };

            return option;
        }

        private Stream GetResourceStream(RunEnvironment runEnvironment)
        {
            const string baseResource = "ArtifactStore.Configs.";
            const string configSufix = "-config.json";

            string resourceId = baseResource + runEnvironment switch
            {
                RunEnvironment.Unknown => "unknown",
                RunEnvironment.Local => "local",
                RunEnvironment.Dev => "dev",
                RunEnvironment.PreProd => "preProd",
                RunEnvironment.Prod => "prod",

                _ => throw new ArgumentException($"Unknown RunEnvironment=(int){(int)runEnvironment}"),
            } + configSufix;

            return Assembly.GetAssembly(typeof(OptionBuilder))!
                .GetManifestResourceStream(resourceId)
                .VerifyNotNull($"Resource {resourceId} not found in assembly");
        }
    }
}
