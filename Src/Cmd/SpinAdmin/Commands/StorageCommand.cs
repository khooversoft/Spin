using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using SpinAdmin.Activities;
using SpinAdmin.Application;
using Toolbox.Tools;

namespace SpinAdmin.Commands
{
    internal class StorageCommand : Command
    {
        public StorageCommand(StorageActivity storageActivity)
            : base("storage", "Edit storage configurations")
        {
            storageActivity.VerifyNotNull(nameof(storageActivity));

            AddCommand(Delete(storageActivity));
            AddCommand(List(storageActivity));
        }

        private Command Delete(StorageActivity storageActivity)
        {
            var cmd = new Command("delete", "Delete storage configuration")
            {
                CommandHelper.StoreOption(),
                CommandHelper.EnvironmentOption(),
                new Option<string>("--channel", "Name of channel"),
            };

            cmd.AddRequiredArguments("--store", "--environment", "--channel");

            cmd.Handler = CommandHandler.Create(async (string store, string environment, string channel, CancellationToken token) =>
            {
                await storageActivity.Delete(store, environment, channel, token);
                return 0;
            });

            return cmd;
        }

        static private Command List(StorageActivity storageActivity)
        {
            var cmd = new Command("list", "List queue's in configuration")
            {
                CommandHelper.StoreOption(),
                CommandHelper.EnvironmentOption(),
            };

            cmd.AddRequiredArguments("--store", "--environment");

            cmd.Handler = CommandHandler.Create(async (string store, string environment, CancellationToken token) =>
            {
                await storageActivity.List(store, environment, token);
                return 0;
            });

            return cmd;
        }
    }
}