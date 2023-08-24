using System.Diagnostics.Contracts;
using System.Reflection.Metadata;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SoftBank.sdk.Models;
using SoftBank.sdk.test.Application;
using SpinCluster.sdk.Actors.Contract;
using SpinCluster.sdk.Actors.PrincipalKey;
using SpinCluster.sdk.Actors.SoftBank;
using Toolbox.Block;
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

    //[Fact]
    //public async Task ConstructAccountWithLedgerItems()
    //{
    //    var softBank = await _softBankFactory.Create(_accountObjectId, _owner, _context).Return();

    //    var accountDetail = new AccountDetail
    //    {
    //        ObjectId = _accountObjectId.ToString(),
    //        OwnerId = _owner,
    //        Name = "Softbank 1",
    //    };

    //    accountDetail.Validate().IsOk().Should().BeTrue();

    //    var detailWrite = await softBank.AccountDetail.Set(accountDetail, _context);
    //    detailWrite.StatusCode.IsOk().Should().BeTrue();

    //    Option signResult = await softBank.ValidateBlockChain(_context);
    //    signResult.StatusCode.IsOk().Should().BeTrue();


    //    var ledgerItems = new[]
    //    {
    //        new LedgerItem { OwnerId = _owner, Description = "Ledger 1", Type = LedgerType.Credit, Amount = 100.0m },
    //        new LedgerItem { OwnerId = _owner, Description = "Ledger 2", Type = LedgerType.Credit, Amount = 55.15m },
    //        new LedgerItem { OwnerId = _owner, Description = "Ledger 3", Type = LedgerType.Debit, Amount = 20.00m }
    //    };

    //    await ledgerItems.ForEachAsync(async x => await softBank.LedgerItems.Add(x, _context).ThrowOnError());

    //    signResult = await softBank.ValidateBlockChain(_context);
    //    signResult.StatusCode.IsOk().Should().BeTrue();

    //    AccountDetail readAccountDetail = softBank.AccountDetail.Get(_owner, _context).Return();
    //    (accountDetail == readAccountDetail).Should().BeTrue();

    //    IReadOnlyList<LedgerItem> readLedgerItems = softBank.LedgerItems.GetReader(_owner, _context).Return().List();
    //    readLedgerItems.Count.Should().Be(3);
    //    ledgerItems.SequenceEqual(readLedgerItems).Should().BeTrue();

    //    decimal balance = softBank.LedgerItems.GetBalance(_owner, _context).Return();
    //    balance.Should().Be(135.15m);
    //}

    //[Fact]
    //public async Task ConstructAccountWithLedgerItems2Signers()
    //{
    //    var acl = new BlockAcl
    //    {
    //        Items = new BlockAccess[]
    //        {
    //            new BlockAccess {BlockType = nameof(LedgerItem), PrincipalId = _owner2, Grant = BlockGrant.Write },
    //        },
    //    };

    //    var softBank = await _softBankFactory.Create(_accountObjectId, _owner, acl, _context).Return();

    //    var accountDetail = new AccountDetail
    //    {
    //        ObjectId = _accountObjectId.ToString(),
    //        OwnerId = _owner,
    //        Name = "Softbank 1",
    //    };

    //    accountDetail.Validate().IsOk().Should().BeTrue();

    //    var detailWrite = await softBank.AccountDetail.Set(accountDetail, _context);
    //    detailWrite.StatusCode.IsOk().Should().BeTrue();

    //    Option signResult = await softBank.ValidateBlockChain(_context);
    //    signResult.StatusCode.IsOk().Should().BeTrue();


    //    var ledgerItems = new[]
    //    {
    //        new LedgerItem { OwnerId = _owner, Description = "Ledger 1", Type = LedgerType.Credit, Amount = 100.0m },
    //        new LedgerItem { OwnerId = _owner, Description = "Ledger 2", Type = LedgerType.Credit, Amount = 55.15m },
    //        new LedgerItem { OwnerId = _owner, Description = "Ledger 3", Type = LedgerType.Debit, Amount = 20.00m }
    //    };

    //    await ledgerItems.ForEachAsync(async x => await softBank.LedgerItems.Add(x, _context).ThrowOnError());

    //    signResult = await softBank.ValidateBlockChain(_context);
    //    signResult.StatusCode.IsOk().Should().BeTrue();

    //    var ledgerItems2 = new[]
    //    {
    //        new LedgerItem { OwnerId = _owner2, Description = "Ledger 1-2", Type = LedgerType.Credit, Amount = 200.0m },
    //        new LedgerItem { OwnerId = _owner2, Description = "Ledger 2-2", Type = LedgerType.Credit, Amount = 155.15m },
    //        new LedgerItem { OwnerId = _owner2, Description = "Ledger 3-2", Type = LedgerType.Debit, Amount = 40.00m }
    //    };

    //    await ledgerItems2.ForEachAsync(async x => await softBank.LedgerItems.Add(x, _context).ThrowOnError());

    //    signResult = await softBank.ValidateBlockChain(_context);
    //    signResult.StatusCode.IsOk().Should().BeTrue();

    //    AccountDetail readAccountDetail = softBank.AccountDetail.Get(_owner, _context).Return();
    //    (accountDetail == readAccountDetail).Should().BeTrue();

    //    IReadOnlyList<LedgerItem> readLedgerItems = softBank.LedgerItems.GetReader(_owner, _context).Return().List();
    //    readLedgerItems.Count.Should().Be(6);
    //    ledgerItems.Concat(ledgerItems2).SequenceEqual(readLedgerItems).Should().BeTrue();

    //    decimal balance = softBank.LedgerItems.GetBalance(_owner, _context).Return();
    //    balance.Should().Be(450.30M);
    //}

    private async Task CreateBankAccount()
    {
        await _setupTools.DeleteUser(_cluster.ServiceProvider, _subscriptionId, _tenantId, _principalId);
        await _setupTools.CreateUser(_cluster.ServiceProvider, _subscriptionId, _tenantId, _principalId);

        SoftBankClient softBankClient = _cluster.ServiceProvider.GetRequiredService<SoftBankClient>();

        var existOption = await softBankClient.Exist(_accountId, _context);
        if (existOption.IsOk()) await softBankClient.Delete(_accountId, _context);

        var createRequest = new AccountDetail
        {
            DocumentId = _accountId,
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
