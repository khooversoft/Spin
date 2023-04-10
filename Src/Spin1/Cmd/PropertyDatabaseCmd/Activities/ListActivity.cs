using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Property;

namespace PropertyDatabaseCmd.Activities
{
    internal class ListActivity
    {
        private readonly ILogger<ListActivity> _logger;

        public ListActivity(ILogger<ListActivity> logger)
        {
            _logger = logger;
        }

        public Task List(string file, CancellationToken token)
        {
            file.VerifyNotEmpty(nameof(file));

            using IDisposable scope = _logger.BeginScope(new { Command = nameof(List), File = file });

            PropertyFile db = PropertyFile.ReadFromFile(file, true);

            string line = db.Properties
                .Select((x, i) => $"({i}) {x.Key}=\"{x.Value}\"")
                .Prepend($"Listing properties from database {db.File}...")
                .Prepend("")
                .Aggregate(string.Empty, (a, x) => a += x + Environment.NewLine);

            _logger.LogInformation(line);

            return Task.CompletedTask;
        }
    }
}