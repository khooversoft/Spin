//using Microsoft.Extensions.Logging;
//using Toolbox.Extensions;
//using Toolbox.Logging;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Toolbox.Types.Maybe;

//namespace Toolbox.Block.Test.Scenarios;

//public class BankBroker
//{
//    private readonly IMessageBroker _messageBroker;
//    private readonly ILogger<BankBroker> _logger;
//    private BankAccountBlock _sc = null!;
//    private string _path = null!;
//    private string _principleId = null!;
//    private BankAccountSCActor _bankAccountSCActor = null!;

//    public BankBroker(IMessageBroker messageBroker, ILogger<BankBroker> logger)
//    {
//        _messageBroker = messageBroker;
//        _logger = logger;
//    }

//    // Path = {domain}/{resource}/{command}
//    public Task<BankBroker> Start(BankAccountBlock sc, BankAccountSCActor bankAccountSCActor, string path, string principleId, ScopeContext context)
//    {
//        sc.NotNull();
//        path.NotEmpty();
//        principleId.NotEmpty();
//        context.Location().LogInformation("Starting, sc={scName}, path={path}", sc.AccountName, path);

//        _sc = sc;
//        _path = path;
//        _principleId = principleId;
//        _bankAccountSCActor = bankAccountSCActor;

//        //_messageBroker.AddRoute<PushTransfer, TransferResult>($"{_path}/push", PushCommand);
//        //_messageBroker.AddRoute<ApplyDeposit, TransferResult>($"{_path}/applyDeposit", ApplyDeposit);

//        return Task.FromResult(this);
//    }

//    public Task Stop()
//    {
//        _sc = null!;
//        return Task.CompletedTask;
//    }

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
//        _sc.NotNull(name: "not initialized");
//        command.Verify();
//        command.FromPath.Assert(x => x == _path, x => $"Address validation, toPath={x}, expected {_path}");

//        context.Location().LogInformation("Pushing transfer from path={path}, toPath{toPath}", _path, command.ToPath);

//        var reqeust = new ApplyDeposit
//        {
//            ToPath = command.ToPath,
//            FromPath = command.FromPath,
//            Amount = command.Amount,
//        };

//        _logger.LogInformation("Sending command to toPath={toPath}, command={command}", command.ToPath, command.ToJsonPascal());
//        TransferResult result = await _messageBroker.Call<ApplyDeposit, TransferResult>($"{command.ToPath}/applyDeposit", reqeust, context);
//        if (result.Status != StatusCode.OK) return TransferResult.Error();

//        context.Location().LogInformation("Debit SC");

//        var ledger = new LedgerItem
//        {
//            Description = $"Transfer to: {command.ToPath}",
//            Type = LedgerType.Debit,
//            Amount = command.Amount,
//        };

//        string blockId = _sc.AddLedger(ledger, _principleId);
//        await _bankAccountSCActor.Set(_sc, context);

//        return TransferResult.Ok();
//    }

//    public async Task<TransferResult> ApplyDeposit(ApplyDeposit command, ScopeContext context)
//    {
//        _sc.NotNull(name: "not initialized");
//        command.Verify();
//        command.ToPath.Assert(x => x == _path, x => $"Address validation, toPath={x}, expected {_path}");

//        var ledger = new LedgerItem
//        {
//            Description = $"From: {command.FromPath}",
//            Type = LedgerType.Credit,
//            Amount = command.Amount,
//        };

//        string blockId = _sc.AddLedger(ledger, _principleId);
//        await _bankAccountSCActor.Set(_sc, context);

//        return TransferResult.Ok();
//    }

//}

//public record TransferResult
//{
//    public string Id { get; init; } = Guid.NewGuid().ToString();
//    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
//    public StatusCode Status { get; init; }
//    public string Description { get; init; } = null!;

//    public static TransferResult Ok() => new TransferResult { Status = StatusCode.OK };
//    public static TransferResult Error() => new TransferResult { Status = StatusCode.BadRequest };
//}

//public record PushTransfer
//{
//    public string Id { get; init; } = Guid.NewGuid().ToString();
//    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
//    public required string ToPath { get; init; } = null!;
//    public required string FromPath { get; init; } = null!;
//    public required decimal Amount { get; init; }
//}

//public record ApplyDeposit
//{
//    public string Id { get; init; } = Guid.NewGuid().ToString();
//    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
//    public required string ToPath { get; init; } = null!;
//    public required string FromPath { get; init; } = null!;
//    public required decimal Amount { get; init; }
//}

//public static class PushPullTransferExtensions
//{
//    public static PushTransfer Verify(this PushTransfer subject)
//    {
//        const string msg = "required";
//        subject.NotNull(name: msg);
//        subject.ToPath.NotEmpty(name: msg);
//        subject.FromPath.NotEmpty(name: msg);

//        return subject;
//    }

//    public static ApplyDeposit Verify(this ApplyDeposit subject)
//    {
//        const string msg = "required";
//        subject.NotNull(name: msg);

//        return subject;
//    }
//}
