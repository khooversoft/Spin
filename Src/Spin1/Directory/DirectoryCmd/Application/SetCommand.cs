using DirectoryCmd.Activities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;

namespace DirectoryCmd.Application;

internal class SetCommand : Command
{
    public SetCommand(SetActivity setActivity)
        : base("set", "Set directory entry from file or by property")
    {
        AddCommand(new SetFileCommand(setActivity));
        AddCommand(new SetProperty(setActivity));
    }

    private class SetFileCommand : Command
    {
        public SetFileCommand(SetActivity setActivity)
            : base("file", "Read entry from file")
        {
            AddArgument(new Argument<string>("file", "Read directory entries from file"));

            Handler = CommandHandler.Create(async (string file, CancellationToken token) =>
            {
                await setActivity.SetFile(file, token);
                return 0;
            });
        }
    }

    private class SetProperty : Command
    {
        public SetProperty(SetActivity setActivity)
            : base("property", "Set property of an directory entry")
        {
            AddArgument(new Argument<string>("directoryId", "Directory id of entry"));
            AddArgument(new Argument<string[]>("property", "Set the property for a directory entry to a value (syntax: Property=value[ property=value...]"));

            Handler = CommandHandler.Create(async (string directoryId, string[] property, CancellationToken token) =>
            {
                await setActivity.SetProperty(directoryId, property, token);
                return 0;
            });
        }
    }
}

