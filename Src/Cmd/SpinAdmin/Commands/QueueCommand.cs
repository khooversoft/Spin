using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using SpinAdmin.Activities;
using SpinAdmin.Application;
using Toolbox.Tools;

namespace SpinAdmin.Commands
{
    internal class QueueCommand : Command
    {
        public QueueCommand(QueueActivity queueActivity)
            : base("queue", "Edit queue configurations")
        {
            queueActivity.VerifyNotNull(nameof(queueActivity));

            AddCommand(Delete(queueActivity));
            AddCommand(List(queueActivity));
        }

        static private Command Delete(QueueActivity queueActivity)   
        {
            var cmd = new Command("delete", "Remove storage configuration")
            {
                CommandHelper.StoreOption(),
                CommandHelper.EnvironmentOption(),
                new Option<string>("--channel", "Name of channel"),
            };

            cmd.AddRequiredArguments("--store", "--environment", "--channel");

            cmd.Handler = CommandHandler.Create(async (string store, string environment, string channel, CancellationToken token) =>
            {
                await queueActivity.Delete(store, environment, channel, token);
                return 0;
            });

            return cmd;
        }

        static private Command List(QueueActivity queueActivity)
        {
            var cmd = new Command("list", "List queue's in configuration")
            {
                CommandHelper.StoreOption(),
                CommandHelper.EnvironmentOption(),
            };

            cmd.AddRequiredArguments("--store", "--environment");

            cmd.Handler = CommandHandler.Create(async (string store, string environment, CancellationToken token) =>
            {
                await queueActivity.List(store, environment, token);
                return 0;
            });

            return cmd;
        }
    }
}
