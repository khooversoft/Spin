using Directory.sdk.Configuration;
using Spin.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart_Installment.sdk.Test.Application;

internal class TestingConfiguration
{
    private ApplicationOption? _applicationOption;

    public static TestingConfiguration Instance { get; } = new TestingConfiguration();

    public async Task<ApplicationOption> GetConfiguration()
    {
        return _applicationOption ??= await ReadConfiguration();
    }

    private async Task<ApplicationOption> ReadConfiguration()
    {
        DirectoryConfigurationOption option = await new DirectoryConfigurationBuilder()
            .AddService(SpinService.Contract.ToString())
            .Build();

        var contractOption = option.Services
            .Where(x => x.Key == SpinService.Contract.ToString())
            .Select(x => x.Value)
            .First();

        return new ApplicationOption
        {
            RunEnvironment = option.RunEnvironment,
            DirectoryUrl = option.DirectoryUrl,
            DirectoryApiKey = option.DirectoryApiKey,
            ContractUrl = contractOption.HostUrl,
            ContractApiKey = contractOption.ApiKey,
        };
    }
}
