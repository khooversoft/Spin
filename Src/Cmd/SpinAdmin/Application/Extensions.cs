using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpinAdmin.Application
{
    internal static class Extensions
    {
        public static void AddRequiredArguments(this Command command, params string[] requiredArguments)
        {
            if (requiredArguments.Length == 0) return;

            command.AddValidator(x =>
            {
                foreach(var argument in requiredArguments)
                {
                    if (!x.Children.Contains(argument)) return $"Argument '{argument}' is required";
                }

                return null;
            });
        }
    }
}
