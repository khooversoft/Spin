using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Azure.DataLake;
using Toolbox.Extensions;

namespace Toolbox.Azure.test.Application;

internal static class TestApplication
{
    public static IDatalakeStore GetDatalake(string basePath) =>
        new DatalakeStore(ReadOption(basePath).Datalake, new NullLogger<DatalakeStore>());

    private static AzureTestOption ReadOption(string basePath) => new ConfigurationBuilder()
        .AddJsonFile("TestSettings.json")
        .AddUserSecrets("Toolbox-Azure-test")
        .Build()
        .Bind<AzureTestOption>()
        .Func(x => x with { Datalake = x.Datalake with { BasePath = basePath } });
}
