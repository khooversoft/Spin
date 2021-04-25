using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Tools.Property;

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

            using IDisposable scope = _logger.BeginScope(new { Command = nameof(Delete), File = file, Key = key });

            PropertyFile db = PropertyFile.ReadFromFile(file, true);

            _logger.LogInformation($"Delete property {key} from database {file}...");

            bool removed = db.Properties.Remove(key);
            if (removed) db.WriteToFile(file);

            _logger.LogInformation($"Property {(removed ? "was removed" : "was not found")}");
            return Task.CompletedTask;
        }
    }
}