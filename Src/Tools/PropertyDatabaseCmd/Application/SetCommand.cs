using Microsoft.Extensions.DependencyInjection;
using PropertyDatabaseCmd.Activities;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;

namespace PropertyDatabaseCmd.Application
{
    internal class SetCommand : Command
    {
        public SetCommand(IServiceProvider serviceProvider)
            : base("set", "Write file to artifact")
        {
            AddArgument(new Argument<string>("file", "File for database"));
            AddArgument(new Argument<string>("key", "Key to delete"));
            AddArgument(new Argument<string>("value", "value of property"));

            Handler = CommandHandler.Create(async (string file, string key, string value, CancellationToken token) =>
            {
                await serviceProvider.GetRequiredService<SetActivity>().Set(file, key, value, token);
                return 0;
            });
        }
    }
}