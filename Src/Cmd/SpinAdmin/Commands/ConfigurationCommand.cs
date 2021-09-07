using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using SpinAdmin.Activities;
using SpinAdmin.Application;

namespace SpinAdmin.Commands
{
    internal class ConfigurationCommand : Command
    {
        public ConfigurationCommand(EnvironmentActivity environmentActivity)
            : base("config", "Configuration management")
        {
            AddCommand(List(environmentActivity));
            AddCommand(Delete(environmentActivity));
            AddCommand(Backup(environmentActivity));
            AddCommand(Restore(environmentActivity));
            AddCommand(Publish(environmentActivity));
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

        static private Command Backup(EnvironmentActivity environmentActivity)
        {
            var cmd = new Command("backup", "Backup configuration store")
            {
                CommandHelper.StoreOption(),
                new Option<string>("--file", "Backup file to write to"),
            };

            cmd.AddRequiredArguments("--store");

            cmd.Handler = CommandHandler.Create(async (string store, string file, CancellationToken token) =>
            {
                await environmentActivity.Backup(store, file, token);
                return 0;
            });

            return cmd;
        }

        static private Command Restore(EnvironmentActivity environmentActivity)
        {
            var cmd = new Command("restore", "Restore configuration backup")
            {
                CommandHelper.StoreOption(),
                new Option<string>("--backupFile", "Path to backup file to restore from"),
                new Option<bool>("--resetStore", "Reset store (delete all before restore)"),
            };

            cmd.AddRequiredArguments("--store", "--backupFile");


            cmd.Handler = CommandHandler.Create(async (string store, string backupFile, bool resetStore, CancellationToken token) =>
            {
                await environmentActivity.Restore(store, backupFile, resetStore, token);
                return 0;
            });

            return cmd;
        }

        static private Command Publish(EnvironmentActivity environmentActivity)
        {
            var cmd = new Command("publish", "Publish an environment")
            {
                CommandHelper.StoreOption(),
                CommandHelper.EnvironmentOption(),
           };

            cmd.AddRequiredArguments("--store", "--environment");

            cmd.Handler = CommandHandler.Create(async (string store, string environment, CancellationToken token) =>
            {
                await environmentActivity.Publish(store, environment, token);
                return 0;
            });

            return cmd;
        }
    }
}
