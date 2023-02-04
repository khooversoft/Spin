using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Abstractions.Tools;

namespace DirectoryCmd.Application;

internal class ApplicationOption
{
    public string DirectoryUrl { get; init; } = null!;
    public string ApiKey { get; init; } = null!;
    public bool Trace { get; init; }
}

internal static class ApplicationOptionExtensions
{
    public static ApplicationOption Verify(this ApplicationOption applicationOption)
    {
        applicationOption.NotNull();
        applicationOption.DirectoryUrl.NotEmpty();
        applicationOption.ApiKey.NotEmpty();

        return applicationOption;
    }
}
