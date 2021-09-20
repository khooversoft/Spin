using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Spin.Common.Configuration;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Local;
using Toolbox.Tools.Property;

namespace SpinAdmin.Activities
{
    internal class EnvironmentActivity
    {
        private readonly ConfigurationStore _configurationStore;
        private readonly ILogger<EnvironmentActivity> _logger;

        public EnvironmentActivity(ConfigurationStore configurationStore, ILogger<EnvironmentActivity> logger)
        {
            _configurationStore = configurationStore;
            _logger = logger;
        }

        public async Task List(string store, CancellationToken token)
        {
            IReadOnlyList<string> results = await _configurationStore.List(store, token);

            var list = new[]
            {
                $"{nameof(List)}: Listing environments",
                "",
            }
            .Concat(results.Select(y => $"Environment={y}"));

            _logger.LogInformation(list.Aggregate(string.Empty, (a, x) => a += x + Environment.NewLine));
        }

        public async Task Create(string store, string environment, bool force, CancellationToken token)
        {
            string configurationFile = _configurationStore
                .Environment(store, environment)
                .File
                .GetConfigurationFile();

            if (File.Exists(configurationFile) && !force)
                throw new ArgumentException($"Configuration file {configurationFile} already exist and force was not specified");

            string exampleJson = ReasourceToString("SpinAdmin.Application.spin.environment.example.json");

            await File.WriteAllTextAsync(configurationFile, exampleJson, token);
            await Edit(store, environment, token);
        }

        public Task Edit(string store, string environment, CancellationToken token)
        {
            IPropertyResolver resolver = new PropertyResolver($"environment={environment}".ToDictionary());

            IReadOnlyList<string> configurationFiles = _configurationStore
                .Environment(store, environment)
                .File
                .GetConfigurationFiles(resolver);

            LocalProcess localProcess = new LocalProcessBuilder()
            {
                ExecuteFile = "cmd",
                Arguments = $"/C code {string.Join(" ", configurationFiles)}",
                UseShellExecute = true,
            }.Build(_logger);

            localProcess.Start();

            return Task.CompletedTask;
        }

        public async Task Delete(string store, string environment, CancellationToken token) => await _configurationStore
            .Environment(store, environment)
            .File
            .Delete(token);

        private static string ReasourceToString(string id)
        {
            using Stream stream = Assembly.GetAssembly(typeof(EnvironmentActivity))
                ?.GetManifestResourceStream(id)
                .VerifyNotNull($"Resource {id} not found in assembly")!;

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
