using ArtifactStore.sdk.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Tools;

namespace Identity.Application
{
    public record Option
    {
        public RunEnvironment RunEnvironment { get; init; }

        public ArtifactStoreOption ArtifactStore { get; init; } = null!;
    }

    public static class OptionExtensions
    {
        public static void Verify(this Option option)
        {
            option.VerifyNotNull(nameof(option));
            option.ArtifactStore.Verify();
        }
    }
}
