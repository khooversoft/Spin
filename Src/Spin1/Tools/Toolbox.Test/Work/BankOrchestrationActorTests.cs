using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using Toolbox.Actor;
using Toolbox.Actor.Host;
using Toolbox.Types;
using Xunit;

namespace Toolbox.Test.Work;

public class BankOrchestrationActorTests
{
    public enum BankAccountName
    {
        FirstAcount,
        SecondAcount,
    };

    [Fact]
    public void SimpleBankTransaction()
    {
        using ServiceProvider service = new ServiceCollection()
            .AddSingleton<BankMoveOrchestration>()
            .AddTransient<BankAccount>()
            .AddLogging(config =>
            {
                config.AddDebug();
                config.AddFilter(x => true);
            })
            .AddActor(config =>
            {
                config.Register<IBankAccount, BankAccount>();
            })
            .BuildServiceProvider();

        IBankAccount bankAccount = service.GetActor<IBankAccount>((ActorKey)BankAccountName.FirstAcount.ToString());
        bankAccount.Add(new BankTransaction
        {
            FromAccount = "General",
            Credit = true,
            Amount = 500.00m
        });

        BankMoveOrchestration bankMoveOrchestration = service.GetRequiredService<BankMoveOrchestration>();

        var trx1 = new BankMoveRequest
        {
            FromAccount = BankAccountName.FirstAcount.ToString(),
            ToAccount = BankAccountName.SecondAcount.ToString(),
            Amount = 100.00m,
        };

        BankMoveResult result = bankMoveOrchestration.Run(new PropertyDictionary(), trx1);
        result.StatusCode.Should().Be(StatusCode.Ok);
        result.ToBalance.Should().Be(100.00m);
        result.FromBalance.Should().Be(400.00m);

        IActorService actorService = service.GetRequiredService<IActorService>();
        var firstBank = actorService.GetActor<IBankAccount>((ActorKey)BankAccountName.FirstAcount.ToString());
        var secondBank = actorService.GetActor<IBankAccount>((ActorKey)BankAccountName.SecondAcount.ToString());

        var firstBankBalance = firstBank.GetBalance();
        firstBankBalance.Should().Be(400.00m);

        var secondBankBalance = secondBank.GetBalance();
        secondBankBalance.Should().Be(100.00m);
    }
}

public class BankMoveOrchestration
{
    private readonly IActorService _actorService;
    private readonly ILogger<BankMoveOrchestration> _logger;

    public BankMoveOrchestration(IActorService actorService, ILogger<BankMoveOrchestration> logger)
    {
        _actorService = actorService;
        _logger = logger;
    }

    public BankMoveResult Run(PropertyDictionary context, BankMoveRequest input)
    {
        var trx1 = new BankTransaction
        {
            FromAccount = input.ToAccount,
            Credit = false,
            Amount = input.Amount,
        };

        var fromBank = _actorService.GetActor<IBankAccount>((ActorKey)input.FromAccount);
        fromBank.Add(trx1);
        var fromBalance = fromBank.GetBalance();

        var trx2 = new BankTransaction
        {
            FromAccount = input.FromAccount,
            Credit = true,
            Amount = input.Amount,
        };

        var toBank = _actorService.GetActor<IBankAccount>((ActorKey)input.ToAccount);
        toBank.Add(trx2);
        var toBalance = toBank.GetBalance();

        return new BankMoveResult
        {
            Request = input,
            StatusCode = StatusCode.Ok,
            ToBalance = toBalance,
            FromBalance = fromBalance,
        };
    }
}

public interface IBankAccount : IActor
{
    void Add(BankTransaction trx);
    decimal GetBalance();
}

public class BankAccount : ActorBase, IBankAccount
{
    private readonly ConcurrentQueue<BankTransaction> _ledger = new ConcurrentQueue<BankTransaction>();

    public void Add(BankTransaction trx) => _ledger.Enqueue(trx);

    public decimal GetBalance() => _ledger
        .Sum(x => x.Credit switch { true => x.Amount, _ => -x.Amount });
}

public record BankTransaction
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset Date { get; init; } = DateTimeOffset.Now;
    public string FromAccount { get; init; } = null!;
    public bool Credit { get; init; }
    public decimal Amount { get; init; }
}

public record BankMoveRequest
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset Date { get; init; } = DateTimeOffset.Now;
    public string FromAccount { get; init; } = null!;
    public string ToAccount { get; init; } = null!;
    public decimal Amount { get; init; }
}

public enum StatusCode { Ok, Error }

public record BankMoveResult
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset Date { get; init; } = DateTimeOffset.Now;
    public BankMoveRequest Request { get; init; } = null!;
    public StatusCode StatusCode { get; init; }

    public decimal FromBalance { get; init; }
    public decimal ToBalance { get; init; }
}