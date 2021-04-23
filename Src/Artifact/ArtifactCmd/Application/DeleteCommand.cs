using ArtifactCmd.Activities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;

namespace ArtifactCmd.Application
{
    internal class DeleteCommand : Command
    {
        public DeleteCommand(ConfigOption configOption, IServiceProvider serviceProvider, ILogger<DeleteCommand> logger)
            : base("delete", "Delete artifact")
        {
            AddArgument(new Argument<string>("id", "ID to delete"));

            Handler = CommandHandler.Create(async (string id, CancellationToken token) =>
            {
                if (!configOption.IsValid(logger)) return 1;

                await serviceProvider.GetRequiredService<DeleteActivity>().Delete(id, token);
                return 0;
            });
        }
    }
}