using System.Collections.Generic;
using Toolbox.Application;
using Toolbox.Azure.DataLake.Model;

namespace ArtifactStore.Application
{
    public record Option
    {
        public RunEnvironment Environment { get; init; }

        public string ApiKey { get; init; } = null!;

        public IReadOnlyList<DataLakeNamespace> Stores { get; init; } = null!;

        public string? HostUrl { get; init; }
    }
}