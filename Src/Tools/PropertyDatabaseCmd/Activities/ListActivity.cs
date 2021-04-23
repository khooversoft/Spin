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
            file = Path.ChangeExtension(file, PropertyResolverBuilder.Extension);

            using IDisposable scope = _logger.BeginScope(new { Command = nameof(List), File = file });

            IPropertyResolverBuilder db = new PropertyResolverBuilder()
                .LoadFromFile(file, true);

            var list = new List<string>
            {
                $"Listing properties from database {file}...",
                ""
            };

            int index = 0;
            db.List().ForEach(x => list.Add($"({index++}) {x.Key}=\"{x.Value}\""));

            _logger.LogInformation(list.Aggregate(string.Empty, (a, x) => a += x + Environment.NewLine));

            return Task.CompletedTask;
        }
    }
}