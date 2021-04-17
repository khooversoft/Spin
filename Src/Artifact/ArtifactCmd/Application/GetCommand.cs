using ArtifactCmd.Activities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Application;

namespace ArtifactCmd.Application
{
    internal class GetCommand : Command
    {
        public GetCommand(IServiceProvider serviceProvider)
            : base("get", "Get artifact")
        {
            AddArgument(new Argument<string>("id", "ID to get"));
            AddArgument(new Argument<string>("file", "File to write to"));

            Handler = CommandHandler.Create(async (string id, string file, CancellationToken token) =>
            {
                await serviceProvider.GetRequiredService<GetActivity>().Get(id, file, token);
                return 0;
            });
        }
    }
}
