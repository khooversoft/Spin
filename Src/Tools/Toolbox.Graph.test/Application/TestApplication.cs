//using Toolbox.Test.Application;
using Microsoft.Extensions.Configuration;
using Toolbox.Azure;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Graph.test.Application;

public static class TestApplication
{
    public static DatalakeOption ReadDatalakeOption(string useBasePath) => new ConfigurationBuilder()
        .AddJsonFile("application/TestSettings.json")
        .AddUserSecrets("Toolbox-Azure-test")
        .Build()
        .Get<DatalakeOption>().NotNull()
        .Func(x => x with { BasePath = useBasePath });
}
