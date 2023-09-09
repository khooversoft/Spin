using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SoftBank.sdk.Models;
using SoftBank.sdk.SoftBank;
using SoftBank.sdk.test.Application;
using Toolbox.Extensions;
using Toolbox.Types;

namespace SoftBank.sdk.test.Basic;

public class SimpleTransactions : IClassFixture<ClusterApiFixture>
{
    private readonly ClusterApiFixture _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);
    private readonly SetupTools _setupTools;

    private const string _subscriptionId = "Company8Subscription";
    private const string _tenantId = "company8.com";
    private const string _principalId = "user1@company8.com";
    private const string _accountId = "softbank:company8.com/contract1";


    public SimpleTransactions(ClusterApiFixture fixture)
    {
        _cluster = fixture;
        _setupTools = new SetupTools(_cluster, _context);
    }

    [Fact]
    public async Task ConstructSoftBank()
    {
        await CreateBankAccount();
        await DeleteBankAccount();
    }

    [Fact]
    public async Task ConstructWithLedgerItems()
    {
        SoftBankClient softBankClient = _cluster.ServiceProvider.GetRequiredService<SoftBankClient>();
        await CreateBankAccount();

        LedgerItem ledgerItem = new LedgerItem
        {
            AccountId = _accountId,
            OwnerId = _principalId,
            Description = "Start of account",
            Type = LedgerType.Credit,
            Amount = 100,
        };

        var writeResponse = await softBankClient.AddLedgerItem(_accountId, ledgerItem, _context);
        writeResponse.IsOk().Should().BeTrue();

        Option<IReadOnlyList<LedgerItem>> ledgerItemsOption = await softBankClient.GetLedgerItems(_accountId, _principalId, _context);
        ledgerItemsOption.IsOk().Should().BeTrue();
        ledgerItemsOption.Return().Count().Should().Be(1);

        LedgerItem readLedgerItem = ledgerItemsOption.Return()[0];
        (ledgerItem == readLedgerItem).Should().BeTrue();

        await DeleteBankAccount();
    }

    [Fact]
    public async Task ConstructAccountWithLedgerItems()
    {
        SoftBankClient softBankClient = _cluster.ServiceProvider.GetRequiredService<SoftBankClient>();
        await CreateBankAccount();

        var ledgerItems = new[]
        {
            new LedgerItem { AccountId =_accountId, OwnerId = _principalId, Description = "Ledger 1", Type = LedgerType.Credit, Amount = 100.0m },
            new LedgerItem { AccountId =_accountId, OwnerId = _principalId, Description = "Ledger 2", Type = LedgerType.Credit, Amount = 55.15m },
            new LedgerItem { AccountId =_accountId, OwnerId = _principalId, Description = "Ledger 3", Type = LedgerType.Debit, Amount = 20.00m }
        };

        foreach (var ledgerItem in ledgerItems)
        {
            var writeResponse = await softBankClient.AddLedgerItem(_accountId, ledgerItem, _context);
            writeResponse.IsOk().Should().BeTrue();
        }

        Option<IReadOnlyList<LedgerItem>> ledgerItemsOption = await softBankClient.GetLedgerItems(_accountId, _principalId, _context);
        ledgerItemsOption.IsOk().Should().BeTrue();
        ledgerItemsOption.Return().Count().Should().Be(3);

        var balanceResponse = await softBankClient.GetBalance(_accountId, _principalId, _context);
        balanceResponse.IsOk().Should().BeTrue();
        balanceResponse.Return().Balance.Should().Be(135.15m);

        await DeleteBankAccount();
    }


    private async Task CreateBankAccount()
    {
        await _setupTools.DeleteUser(_subscriptionId, _tenantId, _principalId);
        await _setupTools.CreateUser(_subscriptionId, _tenantId, _principalId);

        SoftBankClient softBankClient = _cluster.ServiceProvider.GetRequiredService<SoftBankClient>();

        var existOption = await softBankClient.Exist(_accountId, _context);
        if (existOption.IsOk()) await softBankClient.Delete(_accountId, _context);

        var createRequest = new AccountDetail
        {
            AccountId = _accountId,
            OwnerId = _principalId,
            Name = "test account"
        };

        var createOption = await softBankClient.Create(createRequest, _context);
        createOption.IsOk().Should().BeTrue();

        var readAccountDetailOption = await softBankClient.GetAccountDetail(_accountId, _principalId, _context);
        readAccountDetailOption.IsOk().Should().BeTrue();

        var readAccountDetail = readAccountDetailOption.Return();
        (createRequest = readAccountDetail).Should().NotBeNull();

    }

    private async Task DeleteBankAccount()
    {
        SoftBankClient softBankClient = _cluster.ServiceProvider.GetRequiredService<SoftBankClient>();

        var deleteOption = await softBankClient.Delete(_accountId, _context);
        deleteOption.IsOk().Should().BeTrue();
    }
}
