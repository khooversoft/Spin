using System.Collections.Generic;
using Toolbox.Application;
using Toolbox.Azure.DataLake.Model;
using Toolbox.Tools;

namespace ArtifactStore.Application
{
    public record Option
    {
        public RunEnvironment Environment { get; init; }

        public string ApiKey { get; init; } = null!;

        public DataLakeNamespaceOption Store { get; init; } = null!;

        public string? HostUrl { get; init; }
    }

    public static class OptionExtensions
    {
        public static void Verify(this Option option)
        {
            option.VerifyNotNull(nameof(option));

            option.ApiKey.VerifyNotEmpty($"{nameof(option.ApiKey)} is required");
            option.Store.Verify();
        }
    }
}