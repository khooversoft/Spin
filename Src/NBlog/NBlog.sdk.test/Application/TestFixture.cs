using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Toolbox.Extensions;
using NBlog.sdk.Application;
using Toolbox.Tools;
using Toolbox.Types;
using Microsoft.Extensions.DependencyInjection;

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
            .Bind<CmdOption>().Assert(x => x.Validate().IsOk(), "Configuration fails validation");

        ServiceProvider = new ServiceCollection()
            .AddLogging()
            .AddSingleton(Option)
            .AddSingleton<PackageBuild>()
            .BuildServiceProvider();
    }

    public CmdOption Option { get; }

    public IServiceProvider ServiceProvider { get; }
}
