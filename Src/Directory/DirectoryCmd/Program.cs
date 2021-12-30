﻿using Directory.sdk.Client;
using DirectoryCmd.Activities;
using DirectoryCmd.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Reflection;
using Toolbox.Application;
using Toolbox.Configuration;
using Toolbox.Extensions;


try
{
    return await Run(args);
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    return 1;
}


async Task<int> Run(string[] args)
{
    Console.WriteLine($"Directory Command - Version {Assembly.GetExecutingAssembly().GetName().Version}");
    Console.WriteLine();

    ApplicationOption option = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"{{ConfigStore}}/Environments/{{runEnvironment}}-Spin.environment.json", JsonFileOption.Enhance)
        .AddCommandLine(args)
        .AddPropertyResolver()
        .Build()
        .Bind<ApplicationOption>()
        .Verify();

    try
    {
        using (ServiceProvider container = BuildContainer(option))
        {
            var rc = new RootCommand()
            {
                container.GetRequiredService<ListCommand>(),
                container.GetRequiredService<GetCommand>(),
                container.GetRequiredService<DeleteCommand>(),
                container.GetRequiredService<SetCommand>(),
            };

            return await rc.InvokeAsync(args);
        }
    }
    finally
    {
        Console.WriteLine();
        Console.WriteLine("Completed");
    }
}

ServiceProvider BuildContainer(ApplicationOption option)
{
    var service = new ServiceCollection();

    service.AddLogging(x =>
    {
        x.AddConsole();
        x.AddDebug();
    });

    service.AddHttpClient<DirectoryClient>(httpClient =>
    {
        httpClient.BaseAddress = new Uri(option.DirectoryUrl);
        httpClient.DefaultRequestHeaders.Add(Constants.ApiKeyName, option.ApiKey);
    });

    service.AddSingleton<ListActivity>();
    service.AddSingleton<GetActivity>();
    service.AddSingleton<DeleteActivity>();
    service.AddSingleton<SetActivity>();

    service.AddSingleton<DeleteCommand>();
    service.AddSingleton<ListCommand>();
    service.AddSingleton<GetCommand>();
    service.AddSingleton<SetCommand>();

    return service.BuildServiceProvider();
}