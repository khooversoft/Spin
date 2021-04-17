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
    internal class DeleteCommand : Command
    {
        public DeleteCommand(IServiceProvider serviceProvider)
            : base("delete", "Delete artifact")
        {
            AddArgument(new Argument<string>("id", "ID to delete"));

            Handler = CommandHandler.Create(async (string id, CancellationToken token) =>
            {
                await serviceProvider.GetRequiredService<DeleteActivity>().Delete(id, token);
                return 0;
            });
        }
    }
}
