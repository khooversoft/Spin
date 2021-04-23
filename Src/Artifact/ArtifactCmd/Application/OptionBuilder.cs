using Microsoft.Extensions.Configuration;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Reflection;
using Toolbox.Application;
using Toolbox.Configuration;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace ArtifactCmd.Application
{
    internal class OptionBuilder
    {
        public ConfigOption Build(string[] args)
        {
            var environmentOption = new Option<RunEnvironment>(new[] { "--environment", "-e" });

            ParseResult result = new Parser(environmentOption).Parse(args);
            OptionResult? optionResult = result.FindResultFor(environmentOption);

            return optionResult?.Tokens?[0]?.Value switch
            {
                string v => Build(v.ToEnvironment()),

                _ => Build(),
            };
        }

        private ConfigOption Build(RunEnvironment runEnvironment = RunEnvironment.Unknown)
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddPropertyResolver("spin-secrets")
                .Func(x => runEnvironment != RunEnvironment.Unknown ? x.AddJsonStream(GetResourceStream(runEnvironment)) : x)
                .Build()
                .Bind<ConfigOption>()
                .Func(x => x with { Environment = x.Environment ?? runEnvironment });
        }

        private Stream GetResourceStream(RunEnvironment runEnvironment)
        {
            string resourceId = "ArtifactCmd.Configs." + runEnvironment.ToResourceId();

            return Assembly.GetAssembly(typeof(OptionBuilder))!
                .GetManifestResourceStream(resourceId)
                .VerifyNotNull($"Resource {resourceId} not found in assembly");
        }
    }
}