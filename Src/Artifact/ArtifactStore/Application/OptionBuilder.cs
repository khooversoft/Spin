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
            .SetConfigStream(GetResourceStream)
            .SetConfigFiles("appsettings.json");

        private Option FinalizeOption(Option option, RunEnvironment runEnvironment)
        {
            option.Verify();

            option = option with
            {
                Environment = runEnvironment,

                Store = new DataLakeNamespaceOption
                {
                    Namespaces = option.Store.Namespaces.Values.ToDictionary(x => x.Namespace, x => x, StringComparer.OrdinalIgnoreCase),
                }
            };

            return option;
        }

        private Stream GetResourceStream(RunEnvironment runEnvironment)
        {
            string resourceId = "ArtifactStore.Configs." + runEnvironment.ToResourceId();

            return Assembly.GetAssembly(typeof(OptionBuilder))!
                .GetManifestResourceStream(resourceId)
                .VerifyNotNull($"Resource {resourceId} not found in assembly");
        }
    }
}
