﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Toolbox.Configuration;
using Toolbox.Extensions;
using Toolbox.Services;
using Toolbox.Tools;

namespace Toolbox.Application
{
    public class OptionBuilder<T> where T : class, new()
    {
        public bool EnableHelp { get; set; }

        public string[]? Args { get; set; }

        public IReadOnlyList<string>? ConfigFiles { get; set; }

        public Func<T, RunEnvironment, T>? Finalize { get; set; }

        public Func<RunEnvironment, Stream>? ConfigStream { get; set; }

        public string? EnvironmentVariables { get; set; }

        public string? PropertyDatabaseId { get; set; }

        public OptionBuilder<T> SetEnableHelp(bool enableHelp = true) => this.Action((Action<OptionBuilder<T>>)(x => x.EnableHelp = enableHelp));

        public OptionBuilder<T> SetArgs(params string[] args) => this.Action(x => x.Args = args?.ToArray());

        public OptionBuilder<T> SetFinalize(Func<T, RunEnvironment, T> finalize) => this.Action(x => x.Finalize = finalize);

        public OptionBuilder<T> SetConfigFiles(params string[] configFiles) => this.Action(x => x.ConfigFiles = configFiles?.ToList());

        public OptionBuilder<T> SetConfigStream(Func<RunEnvironment, Stream> configStream) => this.Action(x => x.ConfigStream = configStream);

        public OptionBuilder<T> SetEnvironmentVariables(string environmentPrefix) => this.Action(x => x.EnvironmentVariables = environmentPrefix);

        public OptionBuilder<T> SetPropertyDatabaseId(string databaseId) => this.Action(x => x.PropertyDatabaseId = databaseId);

        public T? Build()
        {
            string[] switchNames = typeof(T).GetProperties()
                .Where(x => x.PropertyType == typeof(bool))
                .Select(x => x.Name)
                .ToArray();

            string[] args = (Args ?? Array.Empty<string>())
                .Select(x => switchNames.Contains(x, StringComparer.OrdinalIgnoreCase) ? x + "=true" : x)
                .ToArray();

            if (EnableHelp)
            {
                var helpSignals = new[] { "?", "/?", "-?", "--?", "help", "-help", "--help" };
                args = args.Select(x => helpSignals.Any(y => y == x) ? "help=true" : x).ToArray();
            }

            string? environment = null;
            string? secretId = null;
            T option;

            while (true)
            {
                IConfiguration config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .Action(x => ConfigFiles?.ForEach(y => x.AddJsonFile(y)))
                    .Func(x => GetEnvironmentConfig(environment) switch { Stream v => x.AddJsonStream(v), _ => x })
                    .Func(x => secretId.ToNullIfEmpty() switch { string v => x.AddUserSecrets(v), _ => x })
                    .Func(x => PropertyDatabaseId.ToNullIfEmpty() switch { string v => x.AddPropertyResolver(v), _ => x })
                    .Func(x => EnvironmentVariables.ToNullIfEmpty() switch { string v => x.AddEnvironmentVariables(v), _ => x })
                    .AddCommandLine(args)
                    .Build()
                    .ResolveProperties();

                StandardOption standardOption = config.Bind<StandardOption>();

                switch (standardOption)
                {
                    case var v when v.Help:
                        return null;

                    case var v when !v.Environment.IsEmpty() && environment == null:
                        environment = v.Environment;
                        continue;

                    case var v when !v.SecretId.IsEmpty() && secretId == null:
                        secretId = v.SecretId;
                        continue;
                }

                option = config.Bind<T>();
                option = Finalize?.Invoke(option, environment == null ? RunEnvironment.Unknown : environment.ToEnvironment()) ?? option;

                return option;
            }
        }

        private Stream? GetEnvironmentConfig(string? environment)
        {
            if (ConfigStream == null || environment.IsEmpty()) return null;

            return ConfigStream(environment.ToEnvironment());
        }

        private class StandardOption
        {
            public bool Help { get; set; }
            public string? Environment { get; set; }
            public string? SecretId { get; set; }
        }
    }
}