using ArtifactCmd.Activities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;

namespace ArtifactCmd.Application
{
    internal class SetCommand : Command
    {
        public SetCommand(ConfigOption configOption, IServiceProvider serviceProvider, ILogger<SetCommand> logger)
            : base("set", "Write file to artifact")
        {
            AddArgument(new Argument<string>("file", "File to write to"));
            AddArgument(new Argument<string>("id", "Artifact id to write to"));

            Handler = CommandHandler.Create(async (string file, string id, CancellationToken token) =>
            {
                if (!configOption.IsValid(logger)) return 1;

                await serviceProvider.GetRequiredService<SetActivity>().Set(file, id, token);
                return 0;
            });
        }
    }
}