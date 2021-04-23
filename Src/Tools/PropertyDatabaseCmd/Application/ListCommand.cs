using Microsoft.Extensions.DependencyInjection;
using PropertyDatabaseCmd.Activities;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;

namespace PropertyDatabaseCmd.Application
{
    internal class ListCommand : Command
    {
        public ListCommand(IServiceProvider serviceProvider)
            : base("list", "List properties in database")
        {
            AddArgument(new Argument<string>("file", "File for database"));

            Handler = CommandHandler.Create(async (string file, CancellationToken token) =>
            {
                await serviceProvider.GetRequiredService<ListActivity>().List(file, token);
                return 0;
            });
        }
    }
}