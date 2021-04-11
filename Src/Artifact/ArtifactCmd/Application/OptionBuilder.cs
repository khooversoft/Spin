using ArtifactStore.sdk.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Tools;

namespace ArtifactCmd.Application
{
    internal class OptionBuilder : OptionBuilder<Option>
    {
        public OptionBuilder() =>
            SetFinalize(FinalizeOption)
            .SetConfigStream(GetResourceStream);

        private Option FinalizeOption(Option option, RunEnvironment runEnvironment)
        {
            option.Verify();

            option = option with
            {
                Environment = runEnvironment,
            };

            return option;
        }

        private Stream GetResourceStream(RunEnvironment runEnvironment)
        {
            string resourceId = "ArtifactCmd.Configs." + runEnvironment.ToResourceId();

            return Assembly.GetAssembly(typeof(OptionBuilder))!
                .GetManifestResourceStream(resourceId)
                .VerifyNotNull($"Resource {resourceId} not found in assembly");
        }
    }
}
