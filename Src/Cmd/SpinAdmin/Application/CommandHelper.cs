using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpinAdmin.Application
{
    internal static class CommandHelper
    {
        public static Option<string> StoreOption() => new Option<string>(new[] { "--store", "-s" }, "Path to configuration store");

        public static Option<string> EnvironmentOption() => new Option<string>(new[] { "--environment", "-e" }, "Environment to edit");
    }
}
