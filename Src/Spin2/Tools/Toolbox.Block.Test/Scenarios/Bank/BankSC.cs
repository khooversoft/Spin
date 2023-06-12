//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Toolbox.Block.Contract;
//using Toolbox.Block.Test.Scenarios.Bank.Models;
//using Toolbox.DocumentContainer;
//using Toolbox.Extensions;
//using Toolbox.Logging;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Toolbox.Types.Maybe;

//namespace Toolbox.Block.Test.Scenarios.Bank;

//public class BankSC : IContract
//{
//    private readonly IContractHost _host;
//    private readonly IMessageBroker _messageBroker;
//    private readonly ILogger<BankSC> _logger;

//    public BankSC(IContractHost host, ILogger<BankSC> logger)
//    {
//        _host = host;
//        _logger = logger;
//    }


//    public void Setup(IServiceCollection services, IConfiguration configuration)
//    {
//        throw new NotImplementedException();
//    }

//    public Task<StatusCode> Start(ScopeContext context)
//    {
//        throw new NotImplementedException();
//    }

//    public Task<StatusCode> Stop(ScopeContext context)
//    {
//        throw new NotImplementedException();
//    }

//    public BankSC(IContractHost host, BankAccountBlock sc, IMessageBroker messageBroker, ILogger<BankSC> logger)
//    {
//        _host = host;
//        _accountBlock = sc;
//        _messageBroker = messageBroker;
//        _logger = logger;

//        _ownerPrincipleId = sc.GetAccountMaster()
//            .Return("AccountMaster is missing from block", logger)
//            .OwnerPrincipleId;
//    }

//    public BankAccountBlock AccountBlock => _accountBlock;

//    public DocumentId DocumentId => throw new NotImplementedException();

//    /// <summary>
//    /// Push command, move funds from attached SC to remote SC
//    /// 
//    /// Steps:
//    /// (1) Reserve amount in attached SC
//    /// (2) Send "credit" to remote SC from attached SC
//    /// (3) Receive ack from remote
//    /// (4) Release lease
//    /// (5) Debit attached SC
//    /// </summary>
//    /// <param name="command"></param>
//    /// <returns></returns>
//    public async Task<TransferResult> PushCommand(PushTransfer command, ScopeContext context)
//    {
//        _accountBlock.NotNull(name: "not initialized");
//        command.Verify();

//        var reqeust = new ApplyDeposit
//        {
//            ToPath = command.ToPath,
//            FromPath = command.FromPath,
//            Amount = command.Amount,
//        };

//        context.Location().LogInformation("Sending command to toPath={toPath}, command={command}", command.ToPath, command.ToJsonPascal());

//        TransferResult result = await _messageBroker.Call<ApplyDeposit, TransferResult>($"{command.ToPath}/applyDeposit", reqeust, context);
//        if (result.Status != StatusCode.OK) return TransferResult.Error();

//        context.Location().LogInformation("Debit SC");

//        var ledger = new LedgerItem
//        {
//            Description = $"Transfer to: {command.ToPath}",
//            Type = LedgerType.Debit,
//            Amount = command.Amount,
//        };

//        _accountBlock.AddLedger(ledger, _ownerPrincipleId);
//        await _host.Set(_accountBlock, context);

//        return TransferResult.Ok();
//    }

//    public async Task<TransferResult> ApplyDeposit(ApplyDeposit command, ScopeContext context)
//    {
//        _accountBlock.NotNull(name: "not initialized");
//        command.Verify();

//        var ledger = new LedgerItem
//        {
//            Description = $"From: {command.FromPath}",
//            Type = LedgerType.Credit,
//            Amount = command.Amount,
//        };

//        _accountBlock.AddLedger(ledger, _accountBlock.GetAccountMaster().Return().OwnerPrincipleId);
//        await _host.Set(_accountBlock, context);

//        return TransferResult.Ok();
//    }
//}
