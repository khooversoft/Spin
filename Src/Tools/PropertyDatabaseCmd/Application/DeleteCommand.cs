using Microsoft.Extensions.DependencyInjection;
using PropertyDatabaseCmd.Activities;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;

namespace PropertyDatabaseCmd.Application
{
    internal class DeleteCommand : Command
    {
        public DeleteCommand(IServiceProvider serviceProvider)
            : base("delete", "Delete property")
        {
            AddArgument(new Argument<string>("file", "File for database"));
            AddArgument(new Argument<string>("key", "Key to delete"));

            Handler = CommandHandler.Create(async (string file, string key, CancellationToken token) =>
            {
                await serviceProvider.GetRequiredService<DeleteActivity>().Delete(file, key, token);
                return 0;
            });
        }
    }
}