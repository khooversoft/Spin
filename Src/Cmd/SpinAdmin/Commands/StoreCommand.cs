//using System;
//using System.Collections.Generic;
//using System.CommandLine;
//using System.CommandLine.Invocation;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using SpinAdmin.Activities;
//using SpinAdmin.Application;

//namespace SpinAdmin.Commands
//{
//    internal class StoreCommand : Command
//    {
//        public StoreCommand(StoreActivity storeActivity)
//            : base("store", "Manage store")
//        {
//            AddCommand(Backup(storeActivity));
//            AddCommand(Restore(storeActivity));
//        }

//        static private Command Backup(StoreActivity environmentActivity)
//        {
//            var cmd = new Command("backup", "Backup configuration store")
//            {
//                CommandHelper.StoreOption(),
//                new Option<string>("--file", "Backup file to write to"),
//            };

//            cmd.AddRequiredArguments("--store", "--file");

//            cmd.Handler = CommandHandler.Create(async (string store, string file, CancellationToken token) =>
//            {
//                await environmentActivity.Backup(store, file, token);
//                return 0;
//            });

//            return cmd;
//        }

//        static private Command Restore(StoreActivity environmentActivity)
//        {
//            var cmd = new Command("restore", "Restore configuration backup")
//            {
//                CommandHelper.StoreOption(),
//                new Option<string>("--file", "Backup file to restore from"),
//                new Option<bool>("--resetStore", "Reset store (delete all before restore)"),
//            };

//            cmd.AddRequiredArguments("--store", "--backupFile");

//            cmd.Handler = CommandHandler.Create(async (string store, string file, bool resetStore, CancellationToken token) =>
//            {
//                await environmentActivity.Restore(store, file, resetStore, token);
//                return 0;
//            });

//            return cmd;
//        }
//    }
//}
