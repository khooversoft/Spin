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
    internal class ListCommand : Command
    {
        public ListCommand(IServiceProvider serviceProvider)
            : base("list", "List artifacts")
        {
            AddArgument(new Argument<string>("nameSpace", "Namespace to list"));

            Handler = CommandHandler.Create(async (string nameSpace , CancellationToken token) =>
            {
                await serviceProvider.GetRequiredService<ListActivity>().List(nameSpace, token);
                return 0;
            });
        }
    }
}
