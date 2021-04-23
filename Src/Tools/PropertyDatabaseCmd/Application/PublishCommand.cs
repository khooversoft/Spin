using Microsoft.Extensions.DependencyInjection;
using PropertyDatabaseCmd.Activities;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;

namespace PropertyDatabaseCmd.Application
{
    internal class PublishCommand : Command
    {
        public PublishCommand(IServiceProvider serviceProvider)
            : base("publish", "Publish database to secret folder")
        {
            AddArgument(new Argument<string>("file", "File for database"));
            AddArgument(new Argument<string>("secretId", "Secret ID, only alpha numeric, '-', '.'"));

            Handler = CommandHandler.Create(async (string file, string secretId, CancellationToken token) =>
            {
                await serviceProvider.GetRequiredService<PublishActivity>().Publish(file, secretId, token);
                return 0;
            });
        }
    }
}