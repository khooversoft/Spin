using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;

namespace Toolbox.Application
{
    public static class OptionExtensions
    {
        public static void LogConfigurations<T>(this ILogger logger, T option) where T : class
        {
            const int maxWidth = 80;

            string line = option.GetConfigValues()
                .Prepend(new string('=', maxWidth))
                .Prepend("Current configurations")
                .Aggregate(string.Empty, (a, x) => a += x + Environment.NewLine);

            logger.LogInformation(line);
        }
    }
}
