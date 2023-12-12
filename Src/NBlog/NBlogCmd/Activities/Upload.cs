using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.DataLake;
using Toolbox.CommandRouter;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace NBlogCmd.Activities;

internal class Upload : ICommandRoute
{
    private readonly ILogger<Upload> _logger;
    private readonly IDatalakeStore _store;

    public Upload(IDatalakeStore store, ILogger<Upload> logger)
    {
        _store = store.NotNull();
        _logger = logger.NotNull();
    }

    public CommandSymbol CommandSymbol() => new CommandSymbol("Upload", "Update articles and support files to datalake").Action(x =>
    {
        var basePath = x.AddArgument<string>("basePath", "Path for the base folder to update");

        x.SetHandler(Uploadx, basePath);

    });

    private async Task Uploadx(string basePath)
    {
    }
}
