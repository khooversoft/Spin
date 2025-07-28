using System.Reflection;
//using KGraphCmd.Commands;

Console.WriteLine($"KGraphCmd CLI - Version {Assembly.GetExecutingAssembly().GetName().Version}");
Console.WriteLine();

//IHost host = Host.CreateDefaultBuilder(args)
//    .ConfigureLogging(logging =>
//    {
//        logging.ClearProviders();
//        logging.SimpleConsole();
//    })
//    .ConfigureServices(services =>
//    {
//        services.AddSingleton<GraphHostManager>();

//        services.AddCommandCollection("main")
//            .AddCommand<Command>()
//            .AddCommand<SystemSettings>();

//        services.AddCommandCollection("run")
//            .AddCommand<QueryDb>()
//            .AddCommand<GraphDb>()
//            .AddCommand<TransactionLog>()
//            .AddCommand<SystemSettings>();
//    })
//    .Build();

//ICommandRouterHost mainCommand = host.Services.GetCommandRouterHost("main");
//ScopeContext context = host.Services.GetRequiredService<ILogger<Program>>().ToScopeContext();

//await mainCommand.Run(context, args);
