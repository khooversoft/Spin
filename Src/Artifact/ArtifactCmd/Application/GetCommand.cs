using ArtifactCmd.Activities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;

namespace ArtifactCmd.Application
{
    internal class GetCommand : Command
    {
        public GetCommand(ConfigOption configOption, IServiceProvider serviceProvider, ILogger<GetCommand> logger)
            : base("get", "Get artifact")
        {
            AddArgument(new Argument<string>("id", "ID to get"));
            AddArgument(new Argument<string>("file", "File to write to"));

            Handler = CommandHandler.Create(async (string id, string file, CancellationToken token) =>
            {
                if (!configOption.IsValid(logger)) return 1;

                await serviceProvider.GetRequiredService<GetActivity>().Get(id, file, token);
                return 0;
            });
        }
    }
}