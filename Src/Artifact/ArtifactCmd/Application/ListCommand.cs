using ArtifactCmd.Activities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;

namespace ArtifactCmd.Application
{
    internal class ListCommand : Command
    {
        public ListCommand(ConfigOption configOption, IServiceProvider serviceProvider, ILogger<ListCommand> logger)
            : base("list", "List artifacts")
        {
            AddArgument(new Argument<string>("nameSpace", "Namespace to list"));

            Handler = CommandHandler.Create(async (string nameSpace, CancellationToken token) =>
            {
                if (!configOption.IsValid(logger)) return 1;

                await serviceProvider.GetRequiredService<ListActivity>().List(nameSpace, token);
                return 0;
            });
        }
    }
}