using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SoftBank.sdk.Models;
using SoftBank.sdk.SoftBank;
using SoftBank.sdk.test.Application;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Should;
using Toolbox.Types;

namespace SoftBank.sdk.test.Basic;

public class MultiplePrincipals : IClassFixture<ClusterApiFixture>
{
    private record Config(string Sub, string Tenant, string PrincipalId, string AccountId);

    private readonly ClusterApiFixture _cluster;
    private readonly SetupTools _setupTools;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);


    private Config[] _config = new[]
    {
        new Config("Company9Subscription", "company9.com", "user1@company9.com", "softbank:company9.com/account1"),
        new Config("Company10Subscription", "company10.com", "user2@company10.com", "softbank:company10.com/account2"),
    };

    public MultiplePrincipals(ClusterApiFixture fixture)
    {
        _cluster = fixture;
        _setupTools = new SetupTools(_cluster, _context);
    }

    private string GetAccountId() => _config[0].AccountId;
    private string GetOwnerId() => _config[0].PrincipalId;
    private string GetOwnerId2() => _config[1].PrincipalId;

    [Fact]
    public async Task MultipleLedgerItems()
    {
        SoftBankClient softBankClient = _cluster.ServiceProvider.GetRequiredService<SoftBankClient>();

        await DeleteAccounts();
        await CreateAccounts();

        var newItems = new[]
{
            new SbLedgerItem { AccountId = GetAccountId(), OwnerId = GetOwnerId(), Description = "Ledger 1", Type = SbLedgerType.Credit, Amount = 100.0m },
            new SbLedgerItem { AccountId = GetAccountId(), OwnerId = GetOwnerId(), Description = "Ledger 2", Type = SbLedgerType.Credit, Amount = 55.15m },
            new SbLedgerItem { AccountId = GetAccountId(), OwnerId = GetOwnerId2(), Description = "Ledger 3", Type = SbLedgerType.Debit, Amount = 20.00m },
            new SbLedgerItem { AccountId = GetAccountId(), OwnerId = GetOwnerId(), Description = "Ledger 4", Type = SbLedgerType.Credit, Amount = 55.15m },
            new SbLedgerItem { AccountId = GetAccountId(), OwnerId = GetOwnerId2(), Description = "Ledger 5", Type = SbLedgerType.Debit, Amount = 20.00m },
        };

        foreach (var item in newItems)
        {
            var addResponse = await softBankClient.AddLedgerItem(GetAccountId(), item, _context);
            addResponse.StatusCode.IsOk().Should().BeTrue(addResponse.Error);
        }

        Option<IReadOnlyList<SbLedgerItem>> ledgerItems = await softBankClient.GetLedgerItems(GetAccountId(), GetOwnerId(), _context);
        ledgerItems.StatusCode.IsOk().Should().BeTrue(ledgerItems.Error);
        ledgerItems.Return().Count.Should().Be(newItems.Length);
        newItems.SequenceEqual(ledgerItems.Return()).Should().BeTrue();

        // Check non-owner
        var ledgerItems2 = await softBankClient.GetLedgerItems(GetAccountId(), GetOwnerId2(), _context);
        ledgerItems2.IsError().Should().BeTrue();

        Option<SbAccountBalance> balanceResponse = await softBankClient.GetBalance(GetAccountId(), GetOwnerId(), _context);
        balanceResponse.StatusCode.IsOk().Should().BeTrue();
        balanceResponse.Return().PrincipalBalance.Should().Be(170.30m);

        // Clean up
        await DeleteAccounts();
    }

    [Fact]
    public async Task TestUnauthorizedUserWriteAccess()
    {
        SoftBankClient softBankClient = _cluster.ServiceProvider.GetRequiredService<SoftBankClient>();

        await DeleteAccounts();
        await CreateAccounts();

        var blockAcl = new AclBlock
        {
            AccessRights = new[]
            {
                new AccessBlock { BlockType = nameof(SbLedgerItem), PrincipalId = GetOwnerId(), Grant = BlockGrant.Write },
            },
        };

        var aclOption = await softBankClient.SetAcl(GetAccountId(), blockAcl, GetOwnerId(), _context);
        aclOption.IsOk().Should().BeTrue();

        var newItems = new[]
{
            new SbLedgerItem { AccountId = GetAccountId(), OwnerId = GetOwnerId(), Description = "Ledger 1", Type = SbLedgerType.Credit, Amount = 100.0m },
            new SbLedgerItem { AccountId = GetAccountId(), OwnerId = GetOwnerId2(), Description = "Ledger 3", Type = SbLedgerType.Debit, Amount = 20.00m },
        };

        foreach (var item in newItems.WithIndex())
        {
            var addResponse = await softBankClient.AddLedgerItem(GetAccountId(), item.Item, _context);

            switch (item.Index)
            {
                case 0:
                    addResponse.StatusCode.IsOk().Should().BeTrue(addResponse.Error);
                    break;
                case 1:
                    addResponse.StatusCode.IsError().Should().BeTrue(addResponse.Error);
                    break;
            }
        }

        // Clean up
        await DeleteAccounts();
    }

    private async Task DeleteAccounts()
    {
        foreach (var item in _config)
        {
            await _setupTools.DeleteUser(item.Sub, item.Tenant, item.PrincipalId);
            await DeleteBankAccount(item.AccountId);
        }
    }

    private async Task CreateAccounts()
    {
        foreach (var item in _config)
        {
            await _setupTools.CreateUser(item.Sub, item.Tenant, item.PrincipalId);
            await CreateBankAccount(item.AccountId, item.PrincipalId);
        }
    }

    private async Task CreateBankAccount(string accountId, string principalId)
    {
        SoftBankClient softBankClient = _cluster.ServiceProvider.GetRequiredService<SoftBankClient>();

        var existOption = await softBankClient.Exist(accountId, _context);
        if (existOption.IsOk()) await softBankClient.Delete(accountId, _context);

        var createRequest = new SbAccountDetail
        {
            AccountId = accountId,
            OwnerId = principalId,
            Name = "test account",
            AccessRights = new[]
            {
                new AccessBlock { BlockType = nameof(SbLedgerItem), PrincipalId = GetOwnerId2(), Grant = BlockGrant.Write },
            },
        };

        var createOption = await softBankClient.Create(createRequest, _context);
        createOption.IsOk().Should().BeTrue();

        var readAccountDetailOption = await softBankClient.GetAccountDetail(accountId, principalId, _context);
        readAccountDetailOption.IsOk().Should().BeTrue();

        var readAccountDetail = readAccountDetailOption.Return();
        (createRequest = readAccountDetail).NotNull();
    }

    private async Task DeleteBankAccount(string accountId)
    {
        SoftBankClient softBankClient = _cluster.ServiceProvider.GetRequiredService<SoftBankClient>();

        var deleteOption = await softBankClient.Delete(accountId, _context);
    }
}
