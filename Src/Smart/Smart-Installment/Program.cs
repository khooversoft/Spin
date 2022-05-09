using ContractHost.sdk.Event;
using ContractHost.sdk.Host;
using ContractHost.sdk.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Smart_Installment.sdk;
using Toolbox.Extensions;
using Toolbox.Tools;

IContractHost host = await ContractHostBuilder.Create()
    .AddCommand(args)
    .AddEvent<ContractActual>()
    .AddContractServices()
    .Build();

await host.Run();


internal class ContractActual : IEventService
{
    private readonly ILogger<ContractActual> _logger;
    private readonly DocumentContractClient _documentContractClient;
    private readonly ContractHostOption _contractHostOption;

    public ContractActual(ContractHostOption contractHostOption, DocumentContractClient documentContractClient, ILogger<ContractActual> logger)
    {
        _contractHostOption = contractHostOption.NotNull(nameof(contractHostOption));
        _documentContractClient = documentContractClient.NotNull(nameof(documentContractClient));
        _logger = logger.NotNull(nameof(logger));
    }

    [EventName(EventName.Create)]
    public async Task Create(IContractHost runHost, CancellationToken token)
    {
        runHost.NotNull(nameof(runHost));

        _logger.LogInformation("Running checkpoint, eventName={eventName}", runHost.Context.Option.EventName);

        string eventConfig = _contractHostOption
            .EventConfig.NotNull($"{nameof(_contractHostOption.EventConfig)} is required");

        CreateContractOption option = new ConfigurationBuilder()
            .AddJsonFile(eventConfig)
            .AddCommandLine(runHost.Context.Args)
            .Build()
            .Bind<CreateContractOption>()
            .Verify();

        await _documentContractClient.Create(option, token);
    }

    [EventName(EventName.Checkpoint)]
    public async Task Checkpoint(IContractHost runHost, CancellationToken token)
    {
        runHost.NotNull(nameof(runHost));

        InstallmentContract installmentContract = await _documentContractClient.GetContract(token);

        if (installmentContract.Ledger.Balance() >= installmentContract.Principal) return;

        var ledgerEntry = new LedgerRecord
        {
            Type = LedgerType.Credit,
            Amount = installmentContract.Payment,
            TrxType = "Payment"
        };

        await _documentContractClient.Append(ledgerEntry.ToEnumerable(), token);
    }
}
