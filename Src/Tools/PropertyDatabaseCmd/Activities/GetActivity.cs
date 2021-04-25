﻿using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Tools;
using Toolbox.Tools.Property;

namespace PropertyDatabaseCmd.Activities
{
    internal class GetActivity
    {
        private readonly ILogger<GetActivity> _logger;

        public GetActivity(ILogger<GetActivity> logger)
        {
            _logger = logger;
        }

        public Task Get(string file, string key, CancellationToken token)
        {
            file.VerifyNotEmpty(nameof(file));
            key.VerifyNotEmpty(nameof(key));

            using IDisposable scope = _logger.BeginScope(new { Command = nameof(Get), File = file, Key = key });

            PropertyFile db = PropertyFile.ReadFromFile(file, true);

            _logger.LogInformation($"Get property {key} from database {db.File}...");

            if (!db.Properties.TryGetValue(key, out string? value))
                _logger.LogInformation($"Key {key} not found");
            else
                _logger.LogInformation($"Property {key}={value}");

            return Task.CompletedTask;
        }
    }
}