using DirectoryCmd.Activities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;

namespace DirectoryCmd.Application;

internal class IdentityCommand : Command
{
    public IdentityCommand(IServiceProvider serviceProvider)
        : base("identity", "Manage identities")
    {
        AddCommand(new Create(serviceProvider));
        AddCommand(new Delete(serviceProvider));
        AddCommand(new Get(serviceProvider));
        AddCommand(new Set(serviceProvider));
    }

    private class Create : Command
    {
        public Create(IServiceProvider serviceProvider)
            : base("create", "Create an identity record")
        {
            AddArgument(new Argument<string>("directoryId", "Directory id of entry"));
            AddArgument(new Argument<string>("issuer", "Issuer, user email"));

            Handler = CommandHandler.Create(async (string directoryId, string issuer, CancellationToken token) =>
            {
                await serviceProvider.GetRequiredService<IdentityActivity>().Create(directoryId, issuer, token);
                return 0;
            });
        }
    }

    private class Delete : Command
    {
        public Delete(IServiceProvider serviceProvider)
            : base("delete", "Delete an identity")
        {
            AddArgument(new Argument<string>("directoryId", "Directory id of entry"));

            Handler = CommandHandler.Create(async (string directoryId, CancellationToken token) =>
            {
                await serviceProvider.GetRequiredService<IdentityActivity>().Delete(directoryId, token);
                return 0;
            });
        }
    }

    private class Get : Command
    {
        public Get(IServiceProvider serviceProvider)
            : base("get", "Get identity details")
        {
            AddArgument(new Argument<string>("directoryId", "Directory id of entry"));
            AddArgument(new Argument<string[]>("file", "File to write identity details to"));

            Handler = CommandHandler.Create(async (string directoryId, string file, CancellationToken token) =>
            {
                await serviceProvider.GetRequiredService<IdentityActivity>().Get(directoryId, file, token);
                return 0;
            });
        }
    }

    private class Set : Command
    {
        public Set(IServiceProvider serviceProvider)
            : base("set", "Set identity of an directory entry")
        {
            AddArgument(new Argument<string>("file", "Read directory entries from file"));

            Handler = CommandHandler.Create(async (string file, CancellationToken token) =>
            {
                await serviceProvider.GetRequiredService<IdentityActivity>().Set(file, token);
                return 0;
            });
        }
    }
}

