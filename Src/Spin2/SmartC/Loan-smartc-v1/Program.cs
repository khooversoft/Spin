using System.Reflection;
using LoanContract.sdk;

Console.WriteLine($"Loan-smartc-v1 CLI - Version {Assembly.GetExecutingAssembly().GetName().Version}");
Console.WriteLine();

var state2 = await LoanContractStartup.Create(args).Build().Run();

//var state = await new CommandRouterBuilder()
//    .SetArgs(args)
//    .ConfigureAppConfiguration(config => config.AddEnvironmentVariables("SPIN_SMARTC_"))
//    .ConfigureAppConfiguration((config, service) => service.AddSingleton(config.Build().Bind<AppOption>().Verify()))
//    .AddCommand<CreateContract>()
//    .AddCommand<Payment>()
//    .ConfigureService(x =>
//    {
//        x.AddLoanContract();
//        x.AddSpinClusterClients(LogLevel.Warning);
//        x.AddSoftBankClients(LogLevel.Warning);
//    })
//    .Build()
//    .Run();

//Console.WriteLine($"Return state: {state}");
//return state;

