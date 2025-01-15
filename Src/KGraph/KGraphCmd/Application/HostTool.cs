using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Azure;
using Toolbox.Graph;
using Toolbox.Tools;

namespace KGraphCmd.Application;

internal static class HostTool
{
    public static ServiceProvider StartHost(string jsonFile)
    {
        jsonFile.NotEmpty().Assert(x => File.Exists(x), x => $"File {x} does not exist");

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(jsonFile)
            .Build();

        string secretName = configuration["SecretName"].NotEmpty("SecretName is required");
        configuration = new ConfigurationBuilder()
            .AddJsonFile(jsonFile)
            .AddUserSecrets(secretName)
            .Build();

        var services = new ServiceCollection()
            .AddLogging(x => x.AddConsole().AddDebug())
            .AddDatalakeFileStore(configuration.GetSection("Storage"))
            .AddGraphEngine()
            .BuildServiceProvider();

        return services;
    }

}
