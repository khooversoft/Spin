using Azure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Toolbox.Azure;
using Toolbox.Azure.Identity;
using Toolbox.Configuration;
using Toolbox.Extensions;
using Toolbox.Orleans;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

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

        builder.Configuration.AddPropertyResolver();
    }

    public static ISiloBuilder AddTickShareCluster(this ISiloBuilder builder, HostBuilderContext hostContext)
    {
        builder.NotNull();

        string accountConnection = hostContext.Configuration[TsConstants.StorageAccountConnection].NotEmpty();
        string storageCredential = hostContext.Configuration[TsConstants.StorageCredential].NotEmpty();
        DatalakeOption datalakeOption = DatalakeOptionTool.Create(accountConnection, storageCredential);
        datalakeOption.Validate().Assert(x => x.IsOk(), option => $"StorageOption is invalid, errors={option.Error}");

        builder.Services.AddSingleton(datalakeOption);
        builder.Services.AddSingleton<IDatalakeStore, DatalakeStore>();
        builder.Services.AddSingleton<IFileStore, DatalakeFileStoreConnector>();

        builder.Services.AddGrainFileStore();
        builder.Services.AddStoreCollection((services, config) =>
        {
            config.Add(new StoreConfig("system", getFileStoreService));
            config.Add(new StoreConfig("contract", getFileStoreService));
            config.Add(new StoreConfig("nodes", getFileStoreService));
        });

        return builder;

        static IFileStore getFileStoreService(IServiceProvider services, StoreConfig config) => services.GetRequiredService<IFileStore>();
    }
}

