using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Tools.Property;

namespace PropertyDatabaseCmd.Activities
{
    internal class PublishActivity
    {
        private readonly ILogger<PublishActivity> _logger;

        public PublishActivity(ILogger<PublishActivity> logger)
        {
            _logger = logger;
        }

        public Task Publish(string file, string secretId, CancellationToken token)
        {
            file.VerifyNotEmpty(nameof(file));

            secretId
                .VerifyNotEmpty(nameof(secretId))
                .VerifyAssert(x => x.All(y => char.IsLetterOrDigit(y) || y == '-' || y == '.'), $"Invalid secret ID");

            using IDisposable scope = _logger.BeginScope(new { Command = nameof(Publish), File = file, SecretId = secretId });

            PropertyFile db = PropertyFile.ReadFromFile(file, true);
            new PropertySecret(db.Properties).WriteToSecret(secretId);

            _logger.LogInformation($"Publishing database \"{file}\" to secret Id=\"{secretId}\".");
            return Task.CompletedTask;
        }
    }
}