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

            AddCommand(Edit(queueActivity));
            AddCommand(Delete(queueActivity));
            AddCommand(List(queueActivity));
        }

        static private Command Edit(QueueActivity queueActivity)
        {
            var cmd = new Command("set", "Set queue configuration")
            {
                CommandHelper.StoreOption(),
                CommandHelper.EnvironmentOption(),
                new Option<string>("--name-space", "Namespace for service bus"),
                new Option<string>("--name", "Queue name"),
            };

            cmd.AddRequiredArguments("--store", "--environment", "--name-space", "--name");

            cmd.Handler = CommandHandler.Create(async (string store, string environment, string nameSpace, string name, CancellationToken token) =>
            {
                await queueActivity.Set(store, environment, nameSpace, name, token);
                return 0;
            });

            return cmd;
        }

        static private Command Delete(QueueActivity queueActivity)
        {
            var cmd = new Command("delete", "Remove storage configuration")
            {
                CommandHelper.StoreOption(),
                CommandHelper.EnvironmentOption(),
                new Option<string>("--name-space", "Namespace for service bus"),
                new Option<string>("--name", "Queue name"),
            };

            cmd.AddRequiredArguments("--store", "--environment", "--name-space", "--name");

            cmd.Handler = CommandHandler.Create(async (string store, string environment, string nameSpace, string name, CancellationToken token) =>
            {
                await queueActivity.Delete(store, environment, nameSpace, name, token);
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
