using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Tools.PropertyResolver;

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
            file = Path.ChangeExtension(file, PropertyResolverBuilder.Extension);

            using IDisposable scope = _logger.BeginScope(new { Command = nameof(Set), File = file, Key = key, Value = value });

            IPropertyResolverBuilder db = new PropertyResolverBuilder()
                .LoadFromFile(file, true);

            db.Set(key, value);
            db.Build(file);

            _logger.LogInformation($"Set property {key}={value} from database {file}...");

            return Task.CompletedTask;
        }
    }
}
