using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk.test.Application;

public class TestFixture
{
    public TestFixture()
    {
        var userSecretName = new ConfigurationBuilder()
            .AddJsonFile("test-appsettings.json")
            .Build()
            .Bind<UserSecretName>().Assert(x => x.UserSecrets.IsNotEmpty(), "Secret not set");

        Option = new ConfigurationBuilder()
            .AddJsonFile("test-appsettings.json")
            .AddUserSecrets(userSecretName.UserSecrets.NotEmpty())
            .Build()
            .Bind<StorageOption>().Assert(x => x.Validate().IsOk(), "Configuration fails validation");

        ServiceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton(Option)
            .AddSingleton<PackageBuild>()
            .AddSingleton<PackageUpload>()
            .BuildServiceProvider();
    }

    public StorageOption Option { get; }

    public IServiceProvider ServiceProvider { get; }
}
