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
    internal class SetCommand : Command
    {
        public SetCommand(IServiceProvider serviceProvider)
            : base("set", "Write file to artifact")
        {
            AddArgument(new Argument<string>("file", "File to write to"));
            AddArgument(new Argument<string>("id", "Artifact id to write to"));

            Handler = CommandHandler.Create(async (string file, string id, CancellationToken token) =>
            {
                await serviceProvider.GetRequiredService<SetActivity>().Set(file, id, token);
                return 0;
            });
        }
    }
}
