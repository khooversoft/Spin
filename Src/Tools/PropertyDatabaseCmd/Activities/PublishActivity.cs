using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.PropertyResolver;

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

            file = Path.ChangeExtension(file, PropertyResolverBuilder.Extension);

            using IDisposable scope = _logger.BeginScope(new { Command = nameof(Publish), File = file, SecretId = secretId });

            IPropertyResolverBuilder db = new PropertyResolverBuilder()
                .LoadFromFile(file, true);

            db.BuildForSecretId(secretId);

            _logger.LogInformation($"Publishing database \"{file}\" to secret Id=\"{secretId}\".");
            return Task.CompletedTask;
        }
    }
}