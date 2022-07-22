using Directory.sdk.Client;
using Directory.sdk.Model;
using Directory.sdk.Tools;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Application;
using Toolbox.Configuration;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Directory.sdk.Configuration;

public class DirectoryConfigurationBuilder
{
    public string[]? Args { get; set; }
    public IList<string> ServiceNames { get; } = new List<string>();

    public DirectoryConfigurationBuilder SetArgs(string[]? args) => this.Action(_ => Args = args);
    public DirectoryConfigurationBuilder AddService(string serviceName) => this.Action(_ => ServiceNames.Add(serviceName.NotEmpty()));

    public async Task<DirectoryConfigurationOption> Build()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder();
        return await Build(builder);
    }

    public async Task<DirectoryConfigurationOption> Build(IConfigurationBuilder builder)
    {
        builder.NotNull();

        DirectoryConfigurationOption option = builder
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"{{ConfigStore}}/Environments/{{runEnvironment}}-Spin.resource.json", JsonFileOption.Enhance)
            .Action(x => Args?.Action(_ => x.AddCommandLine(Args)))
            .AddPropertyResolver()
            .Build()
            .Bind<DirectoryConfigurationOption>()
            .Verify();

        return await GetServiceConfigurations(option);
    }

    private async Task<DirectoryConfigurationOption> GetServiceConfigurations(DirectoryConfigurationOption option)
    {
        if (ServiceNames.Count == 0) return option;

        DirectoryConfigurationOption applicationOption = await DirectoryTools.Run(option.DirectoryUrl, option.DirectoryApiKey, async client =>
        {
            ServiceRecord[] services = await ServiceNames
                .Select(x => client.GetServiceRecord(option.RunEnvironment, x))
                .WhenAll();

            return option with
            {
                Services = ServiceNames
                    .Zip(services)
                    .Select(x => new KeyValuePair<string, ServiceRecord>(x.First, x.Second))
                    .ToList()
            };
        });

        return applicationOption;
    }
}
