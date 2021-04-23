using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Tools.PropertyResolver;

namespace PropertyDatabaseCmd.Activities
{
    internal class DeleteActivity
    {
        private readonly ILogger<DeleteActivity> _logger;

        public DeleteActivity(ILogger<DeleteActivity> logger)
        {
            _logger = logger;
        }

        public Task Delete(string file, string key, CancellationToken token)
        {
            file.VerifyNotEmpty(nameof(file));
            key.VerifyNotEmpty(nameof(key));
            file = Path.ChangeExtension(file, PropertyResolverBuilder.Extension);

            using IDisposable scope = _logger.BeginScope(new { Command = nameof(Delete), File = file, Key = key });

            IPropertyResolverBuilder db = new PropertyResolverBuilder()
                .LoadFromFile(file, true);

            _logger.LogInformation($"Delete property {key} from database {file}...");

            bool removed = db.Remove(key);
            db.Build(file);
            _logger.LogInformation($"Property {(removed ? "was removed" : "was not found")}");

            return Task.CompletedTask;
        }
    }
}