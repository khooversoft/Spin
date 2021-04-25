using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Tools.Property;

namespace PropertyDatabaseCmd.Activities
{
    internal class SetActivity
    {
        private readonly ILogger<SetActivity> _logger;

        public SetActivity(ILogger<SetActivity> logger)
        {
            _logger = logger;
        }

        public Task Set(string file, string key, string value, CancellationToken token)
        {
            file.VerifyNotEmpty(nameof(file));
            key.VerifyNotEmpty(nameof(key));
            value.VerifyNotEmpty(nameof(value));

            using IDisposable scope = _logger.BeginScope(new { Command = nameof(Set), File = file, Key = key, Value = value });

            PropertyFile db = PropertyFile.ReadFromFile(file, true);

            db.Properties[key] = value;
            db.WriteToFile();

            _logger.LogInformation($"Set property {key}={value} from database {db.File}...");

            return Task.CompletedTask;
        }
    }
}