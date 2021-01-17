using System.Collections.Generic;
using Toolbox.Application;
using Toolbox.Azure.DataLake.Model;

namespace ArtifactStore.Application
{
    public record Option
    {
        public RunEnvironment RunEnvironment { get; init; }

        public string ApiKey { get; init; } = null!;

        public DataLakeNamespaceOption Store { get; init; } = null!;
    }
}