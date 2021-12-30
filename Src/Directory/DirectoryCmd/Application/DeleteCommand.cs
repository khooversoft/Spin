using Directory.sdk.Client;
using DirectoryCmd.Activities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;

namespace DirectoryCmd.Application;

internal class DeleteCommand : Command
{
    public DeleteCommand(IServiceProvider serviceProvider)
        : base("delete", "Delete a directory entry or directory property")
    {
        AddCommand(new EntryCommand(serviceProvider));
        AddCommand(new Property(serviceProvider));
    }

    private class EntryCommand : Command
    {
        public EntryCommand(IServiceProvider serviceProvider)
            : base("entry", "Delete a directory entry")
        {
            AddArgument(new Argument<string>("directoryId", "Directory Id to delete"));

            Handler = CommandHandler.Create(async (string directoryId, CancellationToken token) =>
            {
                await serviceProvider.GetRequiredService<DeleteActivity>().DeleteEntry(directoryId, token);
                return 0;
            });
        }
    }

    private class Property : Command
    {
        public Property(IServiceProvider serviceProvider)
            : base("property", "Delete a property in a directory entry")
        {
            AddArgument(new Argument<string>("directoryId", "Directory id of entry"));
            AddArgument(new Argument<string[]>("property", "Property name to delete from entry (syntax: {propertyName}[ {propertyName}...]"));

            Handler = CommandHandler.Create(async (string directoryId, string[] property, CancellationToken token) =>
            {
                await serviceProvider.GetRequiredService<DeleteActivity>().DeleteProperty(directoryId, property, token);
                return 0;
            });
        }
    }
}
