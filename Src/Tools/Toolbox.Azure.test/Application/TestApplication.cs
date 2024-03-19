using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Azure;
using Toolbox.Extensions;

namespace Toolbox.Azure.test.Application;

internal static class TestApplication
{
    public static IDatalakeStore GetDatalake(string basePath) =>
        new DatalakeStore(ReadOption(basePath), new NullLogger<DatalakeStore>());

    private static DatalakeOption ReadOption(string basePath) => new ConfigurationBuilder()
        .AddJsonFile("TestSettings.json")
        .AddUserSecrets("Toolbox-Azure-test")
        .Build()
        .Bind<DatalakeOption>()
        .Func(x => x with { BasePath = basePath });
}
