using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using SpinAdmin.Activities;
using SpinAdmin.Application;
using Toolbox.Tools;

namespace SpinAdmin.Commands
{
    internal class SecretCommand : Command
    {
        public SecretCommand(SecretActivity secretActivity)
            : base("secret", "Edit secret used for publish")
        {
            secretActivity.VerifyNotNull(nameof(secretActivity));

            AddCommand(Edit(secretActivity));
            AddCommand(Delete(secretActivity));
            AddCommand(List(secretActivity));
        }

        static private Command Edit(SecretActivity secretActivity)
        {
            var cmd = new Command("set", "Set secret in configuration")
            {
                CommandHelper.StoreOption(),
                CommandHelper.EnvironmentOption(),
                new Option<string>("--key", "Secret key"),
                new Option<string>("--secret", "Secret data"),
            };

            cmd.AddRequiredArguments("--store", "--environment", "--key", "--secret");

            cmd.Handler = CommandHandler.Create(async (string store, string environment, string key, string secret, CancellationToken token) =>
            {
                await secretActivity.Set(store, environment, key, secret, token);
                return 0;
            });

            return cmd;
        }

        static private Command Delete(SecretActivity secretActivity)
        {
            var cmd = new Command("delete", "Remove secret in configuration")
            {
                CommandHelper.StoreOption(),
                CommandHelper.EnvironmentOption(),
                new Option<string>("--key", "Secret key"),
            };

            cmd.AddRequiredArguments("--store", "--environment", "--key");

            cmd.Handler = CommandHandler.Create(async (string store, string environment, string key, CancellationToken token) =>
            {
                await secretActivity.Delete(store, environment, key, token);
                return 0;
            });

            return cmd;
        }

        static private Command List(SecretActivity secretActivity)
        {
            var cmd = new Command("list", "List secret's in configuration")
            {
                CommandHelper.StoreOption(),
                CommandHelper.EnvironmentOption(),
            };

            cmd.AddRequiredArguments("--store", "--environment");

            cmd.Handler = CommandHandler.Create(async (string store, string environment, CancellationToken token) =>
            {
                await secretActivity.List(store, environment, token);
                return 0;
            });

            return cmd;
        }
    }
}
