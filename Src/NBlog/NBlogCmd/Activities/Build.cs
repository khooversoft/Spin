using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace NBlogCmd.Activities;

internal class Build : ICommandRoute
{
    private readonly ILogger<Build> _logger;

    public Build(ILogger<Build> logger)
    {
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("Build", "Build NBlog package for uploading").Action(x =>
    {
        var basePath = x.AddArgument<string>("basePath", "Path for the base folder to update");

        x.SetHandler(BuildPackage, basePath);
    });

    private async Task BuildPackage(string basePath)
    {
    }
}
