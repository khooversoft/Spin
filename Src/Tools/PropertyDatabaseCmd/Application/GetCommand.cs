using Microsoft.Extensions.DependencyInjection;
using PropertyDatabaseCmd.Activities;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;

namespace PropertyDatabaseCmd.Application
{
    internal class GetCommand : Command
    {
        public GetCommand(IServiceProvider serviceProvider)
            : base("get", "Get property")
        {
            AddArgument(new Argument<string>("file", "File for database"));
            AddArgument(new Argument<string>("key", "Key to get"));

            Handler = CommandHandler.Create(async (string file, string key, CancellationToken token) =>
            {
                await serviceProvider.GetRequiredService<GetActivity>().Get(file, key, token);
                return 0;
            });
        }
    }
}