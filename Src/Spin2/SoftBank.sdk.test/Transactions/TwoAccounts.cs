using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SoftBank.sdk.Models;
using SoftBank.sdk.SoftBank;
using SoftBank.sdk.test.Application;
using SoftBank.sdk.Trx;
using SpinTestTools.sdk.ObjectBuilder;
using Toolbox.Types;

namespace SoftBank.sdk.test.Transactions;

public class TwoAccounts : IClassFixture<ClusterApiFixture>
{
    private readonly ClusterApiFixture _cluster;
    private readonly SetupTools _setupTools;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    private readonly ObjectBuilderOption _option = new ObjectOptionBuilder()
        .AddSubscription("Company20Subscription")
        .AddSubscription("Company21Subscription")
        .AddTenant("Company20Subscription", "company20.com")
        .AddTenant("Company21Subscription", "company21.com")
        .AddUser("user1@company20.com")
        .AddUser("user2@company21.com")
        .AddAccount("softbank:company20.com/account1", "user1@company20.com")
        .AddAccount("softbank:company21.com/account2", "user2@company21.com", "user1@company20.com")
        .Build();

    public TwoAccounts(ClusterApiFixture fixture)
    {
        _cluster = fixture;
        _setupTools = new SetupTools(_cluster, _context);

        _option.Validate().ThrowOnError();
    }

    private SbAccountDetail GetAccount1() => _option.SbAccounts[0];
    private SbAccountDetail GetAccount2() => _option.SbAccounts[1];

    [Fact]
    public async Task PushTransferBetweenTwoAccount()
    {
        var objectBuild = new TestObjectBuilder()
            .SetService(_cluster.ServiceProvider)
            .SetOption(_option)
            .AddStandard();

        var buildResult = await objectBuild.Build(_context);
        buildResult.IsOk().Should().BeTrue();

        await AddLedgerToAccount(GetAccount1(), 100, 55.15m);
        var accountBalance1 = await GetBalance(GetAccount1());
        accountBalance1.Should().Be(155.15m);
        await VerifyLedgers(GetAccount1(), 100, 55.15m);

        await AddLedgerToAccount(GetAccount2(), 100);
        var accountBalance2 = await GetBalance(GetAccount2());
        accountBalance2.Should().Be(100);
        await VerifyLedgers(GetAccount2(), 100);

        await Transfer(TrxType.Push, 50.45m, GetAccount1(), GetAccount2());

        accountBalance1 = await GetBalance(GetAccount1());
        accountBalance1.Should().Be(104.7m);
        await VerifyLedgers(GetAccount1(), 100, 55.15m, -50.45m);

        accountBalance2 = await GetBalance(GetAccount2());
        accountBalance2.Should().Be(150.45m);
        await VerifyLedgers(GetAccount2(), 100, 50.45m);

        await objectBuild.DeleteAll(_context);
    }

    [Fact]
    public async Task PullTransferBetweenTwoAccount()
    {
        var objectBuild = new TestObjectBuilder()
            .SetService(_cluster.ServiceProvider)
            .SetOption(_option)
            .AddStandard();

        var buildResult = await objectBuild.Build(_context);
        buildResult.IsOk().Should().BeTrue();

        await AddLedgerToAccount(GetAccount1(), 200, 55.15m, -25.00m);
        var accountBalance1 = await GetBalance(GetAccount1());
        accountBalance1.Should().Be(230.15m);
        await VerifyLedgers(GetAccount1(), 200, 55.15m, -25.00m);

        await AddLedgerToAccount(GetAccount2(), 100, -75, 105.55m);
        var accountBalance2 = await GetBalance(GetAccount2());
        accountBalance2.Should().Be(130.55m);
        await VerifyLedgers(GetAccount2(), 100, -75, 105.55m);

        await Transfer(TrxType.Pull, 12.00m, GetAccount1(), GetAccount2());

        accountBalance1 = await GetBalance(GetAccount1());
        accountBalance1.Should().Be(242.15m);
        await VerifyLedgers(GetAccount1(), 200, 55.15m, -25.00m, 12);

        accountBalance2 = await GetBalance(GetAccount2());
        accountBalance2.Should().Be(118.55m);
        await VerifyLedgers(GetAccount2(), 100, -75, 105.55m, -12);

        await objectBuild.DeleteAll(_context);
    }

    private async Task AddLedgerToAccount(SbAccountDetail config, params decimal[] amounts)
    {
        SoftBankClient softBankClient = _cluster.ServiceProvider.GetRequiredService<SoftBankClient>();

        foreach (var amount in amounts)
        {
            var ledger = new SbLedgerItem
            {
                AccountId = config.AccountId,
                OwnerId = config.OwnerId,
                Description = "Ledger-" + Guid.NewGuid().ToString(),
                Type = amount > 0 ? SbLedgerType.Credit : SbLedgerType.Debit,
                Amount = Math.Abs(amount),
            };

            var addResponse = await softBankClient.AddLedgerItem(config.AccountId, ledger, _context);
            addResponse.StatusCode.IsOk().Should().BeTrue(addResponse.Error);
        }
    }

    private async Task Transfer(TrxType type, decimal amount, SbAccountDetail from, SbAccountDetail to)
    {
        SoftBankTrxClient client = _cluster.ServiceProvider.GetRequiredService<SoftBankTrxClient>();

        var request = new TrxRequest
        {
            PrincipalId = from.OwnerId,
            AccountID = from.AccountId,
            PartyAccountId = to.AccountId,
            Description = "test",
            Type = type,
            Amount = amount,
        };

        Option<TrxResponse> result = await client.Request(request, _context);
        result.IsOk().Should().BeTrue(result.StatusCode.ToString());

        TrxResponse trxResponse = result.Return();
        trxResponse.Request.Should().Be(request);
        trxResponse.Status.Should().Be(TrxStatusCode.Completed);
        trxResponse.Amount.Should().Be(amount);
        trxResponse.Error.Should().BeNull();
        trxResponse.SourceLedgerItemId.Should().NotBeNullOrEmpty();
        trxResponse.DestinationLedgerItemId.Should().NotBeNullOrEmpty();
    }

    private async Task<decimal> GetBalance(SbAccountDetail config)
    {
        SoftBankClient softBankClient = _cluster.ServiceProvider.GetRequiredService<SoftBankClient>();
        var balanceResult = await softBankClient.GetBalance(config.AccountId, config.OwnerId, _context);
        balanceResult.IsOk().Should().BeTrue();

        SbAccountBalance result = balanceResult.Return();
        result.DocumentId.Should().Be(config.AccountId);
        return result.Balance;
    }

    private async Task VerifyLedgers(SbAccountDetail config, params decimal[] amounts)
    {
        SoftBankClient client = _cluster.ServiceProvider.GetRequiredService<SoftBankClient>();

        var response = await client.GetLedgerItems(config.AccountId, config.OwnerId, _context);
        response.IsOk().Should().BeTrue();

        IReadOnlyList<SbLedgerItem> ledgerItems = response.Return();
        var ledgerAmounts = ledgerItems.Select(x => x.NaturalAmount).ToArray();
        amounts.SequenceEqual(ledgerAmounts).Should().BeTrue();
    }
}