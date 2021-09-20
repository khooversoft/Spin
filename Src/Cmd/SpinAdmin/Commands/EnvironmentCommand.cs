using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using SpinAdmin.Activities;
using SpinAdmin.Application;

namespace SpinAdmin.Commands
{
    internal class EnvironmentCommand : Command
    {
        public EnvironmentCommand(EnvironmentActivity environmentActivity, PublishActivity publishActivity)
            : base("environment", "Environment management")
        {
            AddCommand(List(environmentActivity));
            AddCommand(Edit(environmentActivity));
            AddCommand(Delete(environmentActivity));
            AddCommand(Publish(publishActivity));
        }

        static private Command List(EnvironmentActivity environmentActivity)
        {
            var cmd = new Command("list", "List environment configurations")
            {
                CommandHelper.StoreOption()
            };

            cmd.AddRequiredArguments("--store");

            cmd.Handler = CommandHandler.Create(async (string store, CancellationToken token) =>
            {
                await environmentActivity.List(store, token);
                return 0;
            });

            return cmd;
        }

        static private Command Edit(EnvironmentActivity environmentActivity)
        {
            var cmd = new Command("edit", "Edit an environment")
            {
                CommandHelper.StoreOption(),
                CommandHelper.EnvironmentOption(),
            };

            cmd.AddRequiredArguments("--store", "--environment");

            cmd.Handler = CommandHandler.Create(async (string store, string environment, CancellationToken token) =>
            {
                await environmentActivity.Edit(store, environment, token);
                return 0;
            });

            return cmd;
        }

        static private Command Delete(EnvironmentActivity environmentActivity)
        {
            var cmd = new Command("delete", "Delete an environment")
            {
                CommandHelper.StoreOption(),
                CommandHelper.EnvironmentOption(),
            };

            cmd.AddRequiredArguments("--store", "--environment");

            cmd.Handler = CommandHandler.Create(async (string store, string environment, CancellationToken token) =>
            {
                await environmentActivity.Delete(store, environment, token);
                return 0;
            });

            return cmd;
        }

        static private Command Publish(PublishActivity publishActivity)
        {
            var cmd = new Command("publish", "Publish an environment")
            {
                CommandHelper.StoreOption(),
                CommandHelper.EnvironmentOption(),
           };

            cmd.AddRequiredArguments("--store", "--environment");

            cmd.Handler = CommandHandler.Create(async (string store, string environment, CancellationToken token) =>
            {
                await publishActivity.Publish(store, environment, token);
                return 0;
            });

            return cmd;
        }
    }
}
