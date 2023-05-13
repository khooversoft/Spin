using Microsoft.Extensions.Logging;
using Toolbox.Block.Test.Scenarios.Bank.Models;
using Toolbox.DocumentContainer;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Types.Maybe;

namespace Toolbox.Block.Test.Scenarios.Bank;

public class BankSC
{
    private readonly BankHost _host;
    private readonly IMessageBroker _messageBroker;
    private readonly ILogger<BankSC> _logger;
    private readonly BankAccountBlock _accountBlock;
    private readonly string _ownerPrincipleId;

    public BankSC(BankHost host, BankAccountBlock sc, IMessageBroker messageBroker, ILogger<BankSC> logger)
    {
        _host = host;
        _accountBlock = sc;
        _messageBroker = messageBroker;
        _logger = logger;

        _ownerPrincipleId = sc.GetAccountMaster()
            .Return("AccountMaster is missing from block", logger)
            .OwnerPrincipleId;
    }

    public BankAccountBlock AccountBlock => _accountBlock;

    /// <summary>
    /// Push command, move funds from attached SC to remote SC
    /// 
    /// Steps:
    /// (1) Reserve amount in attached SC
    /// (2) Send "credit" to remote SC from attached SC
    /// (3) Receive ack from remote
    /// (4) Release lease
    /// (5) Debit attached SC
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public async Task<TransferResult> PushCommand(PushTransfer command, ScopeContext context)
    {
        _accountBlock.NotNull(name: "not initialized");
        command.Verify();

        var reqeust = new ApplyDeposit
        {
            ToPath = command.ToPath,
            FromPath = command.FromPath,
            Amount = command.Amount,
        };

        _logger.LogInformation(context.Location(), "Sending command to toPath={toPath}, command={command}", command.ToPath, command.ToJsonPascal());

        TransferResult result = await _messageBroker.Send<ApplyDeposit, TransferResult>($"{command.ToPath}/applyDeposit", reqeust, context);
        if (result.Status != StatusCode.OK) return TransferResult.Error();

        _logger.LogInformation(context.Location(), "Debit SC");

        var ledger = new LedgerItem
        {
            Description = $"Transfer to: {command.ToPath}",
            Type = LedgerType.Debit,
            Amount = command.Amount,
        };

        _accountBlock.AddLedger(ledger, _ownerPrincipleId);
        await _host.Set(_accountBlock, context);

        return TransferResult.Ok();
    }

    public async Task<TransferResult> ApplyDeposit(ApplyDeposit command, ScopeContext context)
    {
        _accountBlock.NotNull(name: "not initialized");
        command.Verify();

        var ledger = new LedgerItem
        {
            Description = $"From: {command.FromPath}",
            Type = LedgerType.Credit,
            Amount = command.Amount,
        };

        _accountBlock.AddLedger(ledger, _accountBlock.GetAccountMaster().Return().OwnerPrincipleId);
        await _host.Set(_accountBlock, context);

        return TransferResult.Ok();
    }
}
