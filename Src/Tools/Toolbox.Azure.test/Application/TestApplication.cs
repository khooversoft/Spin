using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;

namespace Toolbox.Azure.test.Application;

internal static class TestApplication
{
    public static IKeyStore GetDatalake(string basePath) => new DatalakeStore(ReadOption(basePath), new NullLogger<DatalakeStore>());

    public static DatalakeOption ReadOption(string basePath) => new ConfigurationBuilder()
        .AddJsonFile("TestSettings.json")
        .AddUserSecrets("Toolbox-Azure-test")
        .Build()
        .Get<DatalakeOption>().NotNull()
        .Func(x => x with { BasePath = basePath });
}
