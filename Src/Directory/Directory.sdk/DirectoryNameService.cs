using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Directory.sdk.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Configuration;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Directory.sdk
{
    public class DirectoryNameService : IDirectoryNameService
    {
        private readonly ILogger<DirectoryNameService> _logger;
        private readonly ConcurrentDictionary<string, Database> _databases = new();
        private Database? _default;

        public DirectoryNameService() => _logger = NullLogger<DirectoryNameService>.Instance;
        public DirectoryNameService(ILogger<DirectoryNameService> logger)
        {
            logger.VerifyNotNull(nameof(logger));

            _logger = logger;
        }

        public Database Get(string domain) => _databases[domain];

        public Database Default { get => _default.VerifyNotNull("Default environment not set"); private set => _default = value; }

        public DirectoryNameService Load(string configStore, string environment, IEnumerable<KeyValuePair<string, string>>? properties = null, params string[] configFiles)
        {
            configStore.VerifyNotEmpty(nameof(configStore));
            environment.VerifyNotEmpty(nameof(environment));

            var dict = new[]
            {
                (nameof(configStore), configStore).ToKeyValuePair(),
                (nameof(environment), environment).ToKeyValuePair(),
            }.Concat(properties ?? Array.Empty<KeyValuePair<string, string>>());

            EnvironmentModel environmentModel = new ConfigurationBuilder()
                .AddInMemoryCollection(dict)
                .AddJsonFile($"{{ConfigStore}}/Environments/{{environment}}-Spin.environment.json", JsonFileOption.Enhance)
                .Action(x => configFiles.ForEach(y => x.AddJsonFile(y)))
                .AddPropertyResolver()
                .Build()
                .Bind<EnvironmentModel>();

            _databases[environment] = new Database(environment).Load(environmentModel);

            _logger.LogInformation($"Loaded environment {environment} from configStore {configStore}");

            return this;
        }

        public Database Select(string domain) => _databases.Get(domain, "domain");

        public Database SelectDefault(string domain) => this.Func(x => Default = Select(domain));
    }
}