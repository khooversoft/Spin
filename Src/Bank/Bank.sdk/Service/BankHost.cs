using Artifact.sdk;
using Directory.sdk.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace Bank.sdk.Service;

public class BankHost : IHostedService
{
    private readonly BankOption _bankOption;
    private readonly DirectoryClient _directoryClient;
    private readonly ArtifactClient _artifactClient;
    private readonly ILoggerFactory _loggerFactory;
    private readonly BankClearingQueue _clearingQueue;
    private readonly BankClearing _bankClearing;
    private readonly BankDocument _bankDocument;
    private readonly BankTransaction _bankTransaction;
    private readonly BankClearingReceiver _bankClearingReceiver;
    private readonly BankDirectory _bankDirectory;

    public BankHost(BankOption bankOption, DirectoryClient directoryClient, ArtifactClient artifactClient, ILoggerFactory loggerFactory)
    {
        _bankOption = bankOption.NotNull(nameof(bankOption));
        _directoryClient = directoryClient.NotNull(nameof(directoryClient));
        _artifactClient = artifactClient.NotNull(nameof(artifactClient));
        _loggerFactory = loggerFactory.NotNull(nameof(loggerFactory));

        _bankDirectory = new BankDirectory(_bankOption, _directoryClient, _loggerFactory);
        _clearingQueue = new BankClearingQueue(_bankOption, _bankDirectory, _loggerFactory.CreateLogger<BankClearingQueue>());
        _bankDocument = new BankDocument(_bankOption, _artifactClient, _loggerFactory.CreateLogger<BankDocument>());

        _bankTransaction = new BankTransaction(_bankOption, _bankDocument, _loggerFactory.CreateLogger<BankTransaction>());
        _bankClearing = new BankClearing(_bankOption, _clearingQueue, _bankTransaction, _loggerFactory.CreateLogger<BankClearing>());
        _bankClearingReceiver = new BankClearingReceiver(_bankClearing, _bankDirectory, _loggerFactory);
    }

    public async Task StartAsync(CancellationToken token)
    {
        await _bankDirectory.LoadDirectory(token);
        await _bankClearingReceiver.StartAsync(token);
    }

    public async Task StopAsync(CancellationToken token) => await _bankClearingReceiver.StopAsync(token);

    public BankTransaction Transaction => _bankTransaction;

    public BankClearing BankClearing => _bankClearing;

    public BankDocument BankDocument => _bankDocument;

    public BankDirectory BankDirectory => _bankDirectory;
}
