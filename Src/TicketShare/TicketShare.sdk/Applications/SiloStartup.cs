using Azure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Azure;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Toolbox.Orleans;
using Toolbox.Identity;

namespace TicketShare.sdk;

public static class SiloStartup
{

    public static void AddApplicationConfiguration(this WebApplicationBuilder builder)
    {
        string connectionString = builder.Configuration.GetConnectionString("AppConfig").NotNull();
        ClientSecretCredential credential = ClientCredential.ToClientSecretCredential(connectionString);

        var appConfigEndpoint = "https://biz-bricks-prod-configuration.azconfig.io";

        // Build configuration
        builder.Configuration.AddAzureAppConfiguration(options =>
        {
            options.Connect(new Uri(appConfigEndpoint), credential)
                .ConfigureKeyVault(kv =>
                {
                    kv.SetCredential(credential);
                })
                .Select(TsConstants.ConfigurationFilter, LabelFilter.Null)
                .Select(TsConstants.ConfigurationFilter, builder.Environment.EnvironmentName);
        });
    }
    
    public static ISiloBuilder AddTickShareCluster(this ISiloBuilder builder, HostBuilderContext hostContext)
    {
        builder.NotNull();

        DatalakeOption datalakeOption = hostContext.Configuration.GetSection(TsConstants.StorageOptionConfigPath).Get<DatalakeOption>().NotNull();
        datalakeOption.Validate().Assert(x => x.IsOk(), option => $"StorageOption is invalid, errors={option.Error}");

        Console.WriteLine($"SiloStartup: option={datalakeOption}");

        builder.Services.AddDatalakeManager(manager =>
        {
            manager.Add("default", datalakeOption);
            manager.AddMap("*", "default");
        });

        builder.AddDatalakeGrainStorage();
        builder.AddIdentityActor();

        return builder;
    }
}

