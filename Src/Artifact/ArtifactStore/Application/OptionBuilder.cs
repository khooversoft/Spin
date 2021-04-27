using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
            .SetConfigFiles("appsettings.json")
            .SetPropertyDatabaseId("spin-secrets");

        private Option FinalizeOption(Option option, RunEnvironment runEnvironment)
        {
            option.Verify();

            option = option with
            {
                Environment = runEnvironment,
                Stores = option.Stores.ToList(),
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