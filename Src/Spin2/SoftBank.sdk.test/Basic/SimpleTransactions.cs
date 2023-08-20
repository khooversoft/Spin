using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SoftBank.sdk.Application;
using SoftBank.sdk.Models;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Types;

namespace SoftBank.sdk.test.Basic;

public class SimpleTransactions
{
    private static readonly PrincipalId _owner = "user@domain.com";
    private static readonly PrincipalId _owner2 = "user2@domain.com";
    private static readonly ObjectId _accountObjectId = $"contract/tenant/{_owner}/account1";
    private static readonly PrincipalSignature _ownerSignature = new PrincipalSignature(_owner, _owner, "userBusiness@domain.com");
    private static readonly PrincipalSignature _ownerSignature2 = new PrincipalSignature(_owner2, _owner2, "userBusiness@domain.com");
    private readonly PrincipalSignatureCollection _signCollection;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);
    private readonly SoftBankFactory _softBankFactory;

    public SimpleTransactions()
    {
        _signCollection = new PrincipalSignatureCollection()
            .Add(_ownerSignature)
            .Add(_ownerSignature2);

        IServiceProvider service = new ServiceCollection()
            .AddLogging()
            .AddSoftBank()
            .AddSingleton<ISign>(_signCollection)
            .AddSingleton<ISignValidate>(_signCollection)
            .BuildServiceProvider();

        _softBankFactory = service.GetRequiredService<SoftBankFactory>();
    }

    [Fact]
    public async Task ConstructTest()
    {
        var softBank = await _softBankFactory.Create(_accountObjectId, _owner, _context).Return();
        Option signResult = await softBank.ValidateBlockChain(_context);
        signResult.StatusCode.IsOk().Should().BeTrue();
    }

    [Fact]
    public async Task ConstructWithAccountDetails()
    {
        var softBank = await _softBankFactory.Create(_accountObjectId, _owner, _context).Return();

        var accountDetail = new AccountDetail
        {
            ObjectId = _accountObjectId.ToString(),
            OwnerId = _owner,
            Name = "Softbank 1",
        };

        accountDetail.Validate().IsOk().Should().BeTrue();

        var detailWrite = await softBank.AccountDetail.Set(accountDetail, _context);
        detailWrite.StatusCode.IsOk().Should().BeTrue();

        Option signResult = await softBank.ValidateBlockChain(_context);
        signResult.StatusCode.IsOk().Should().BeTrue();

        var readAccountDetail = softBank.AccountDetail.Get(_owner, _context);
        readAccountDetail.StatusCode.IsOk().Should().BeTrue();
        (accountDetail == readAccountDetail.Return()).Should().BeTrue();
    }

    [Fact]
    public async Task ConstructWithLedgerItems()
    {
        var softBank = await _softBankFactory.Create(_accountObjectId, _owner, _context).Return();

        LedgerItem ledgerItem = new LedgerItem
        {
            OwnerId = _owner,
            Description = "Start of account",
            Type = LedgerType.Credit,
            Amount = 100,
        };

        ledgerItem.Validate().IsOk().Should().BeTrue();

        var writeResult = await softBank.LedgerItems.Add(ledgerItem, _context);
        writeResult.StatusCode.IsOk().Should().BeTrue();

        Option signResult = await softBank.ValidateBlockChain(_context);
        signResult.StatusCode.IsOk().Should().BeTrue();

        var readledgerItem = softBank.LedgerItems.GetReader(_owner, _context);
        readledgerItem.StatusCode.IsOk().Should().BeTrue();

        readledgerItem.Return().Count.Should().Be(1);
        (ledgerItem == readledgerItem.Return().List().First()).Should().BeTrue();
    }

    [Fact]
    public async Task ConstructAccountWithLedgerItems()
    {
        var softBank = await _softBankFactory.Create(_accountObjectId, _owner, _context).Return();

        var accountDetail = new AccountDetail
        {
            ObjectId = _accountObjectId.ToString(),
            OwnerId = _owner,
            Name = "Softbank 1",
        };

        accountDetail.Validate().IsOk().Should().BeTrue();

        var detailWrite = await softBank.AccountDetail.Set(accountDetail, _context);
        detailWrite.StatusCode.IsOk().Should().BeTrue();

        Option signResult = await softBank.ValidateBlockChain(_context);
        signResult.StatusCode.IsOk().Should().BeTrue();


        var ledgerItems = new[]
        {
            new LedgerItem { OwnerId = _owner, Description = "Ledger 1", Type = LedgerType.Credit, Amount = 100.0m },
            new LedgerItem { OwnerId = _owner, Description = "Ledger 2", Type = LedgerType.Credit, Amount = 55.15m },
            new LedgerItem { OwnerId = _owner, Description = "Ledger 3", Type = LedgerType.Debit, Amount = 20.00m }
        };

        await ledgerItems.ForEachAsync(async x => await softBank.LedgerItems.Add(x, _context).ThrowOnError());

        signResult = await softBank.ValidateBlockChain(_context);
        signResult.StatusCode.IsOk().Should().BeTrue();

        AccountDetail readAccountDetail = softBank.AccountDetail.Get(_owner, _context).Return();
        (accountDetail == readAccountDetail).Should().BeTrue();

        IReadOnlyList<LedgerItem> readLedgerItems = softBank.LedgerItems.GetReader(_owner, _context).Return().List();
        readLedgerItems.Count.Should().Be(3);
        ledgerItems.SequenceEqual(readLedgerItems).Should().BeTrue();

        decimal balance = softBank.LedgerItems.GetBalance(_owner, _context).Return();
        balance.Should().Be(135.15m);
    }

    [Fact]
    public async Task ConstructAccountWithLedgerItems2Signers()
    {
        var acl = new BlockAcl
        {
            Items = new BlockAccess[]
            {
                new BlockAccess {BlockType = nameof(LedgerItem), PrincipalId = _owner2, Grant = BlockGrant.Write },
            },
        };

        var softBank = await _softBankFactory.Create(_accountObjectId, _owner, acl, _context).Return();

        var accountDetail = new AccountDetail
        {
            ObjectId = _accountObjectId.ToString(),
            OwnerId = _owner,
            Name = "Softbank 1",
        };

        accountDetail.Validate().IsOk().Should().BeTrue();

        var detailWrite = await softBank.AccountDetail.Set(accountDetail, _context);
        detailWrite.StatusCode.IsOk().Should().BeTrue();

        Option signResult = await softBank.ValidateBlockChain(_context);
        signResult.StatusCode.IsOk().Should().BeTrue();


        var ledgerItems = new[]
        {
            new LedgerItem { OwnerId = _owner, Description = "Ledger 1", Type = LedgerType.Credit, Amount = 100.0m },
            new LedgerItem { OwnerId = _owner, Description = "Ledger 2", Type = LedgerType.Credit, Amount = 55.15m },
            new LedgerItem { OwnerId = _owner, Description = "Ledger 3", Type = LedgerType.Debit, Amount = 20.00m }
        };

        await ledgerItems.ForEachAsync(async x => await softBank.LedgerItems.Add(x, _context).ThrowOnError());

        signResult = await softBank.ValidateBlockChain(_context);
        signResult.StatusCode.IsOk().Should().BeTrue();

        var ledgerItems2 = new[]
        {
            new LedgerItem { OwnerId = _owner2, Description = "Ledger 1-2", Type = LedgerType.Credit, Amount = 200.0m },
            new LedgerItem { OwnerId = _owner2, Description = "Ledger 2-2", Type = LedgerType.Credit, Amount = 155.15m },
            new LedgerItem { OwnerId = _owner2, Description = "Ledger 3-2", Type = LedgerType.Debit, Amount = 40.00m }
        };

        await ledgerItems2.ForEachAsync(async x => await softBank.LedgerItems.Add(x, _context).ThrowOnError());

        signResult = await softBank.ValidateBlockChain(_context);
        signResult.StatusCode.IsOk().Should().BeTrue();

        AccountDetail readAccountDetail = softBank.AccountDetail.Get(_owner, _context).Return();
        (accountDetail == readAccountDetail).Should().BeTrue();

        IReadOnlyList<LedgerItem> readLedgerItems = softBank.LedgerItems.GetReader(_owner, _context).Return().List();
        readLedgerItems.Count.Should().Be(6);
        ledgerItems.Concat(ledgerItems2).SequenceEqual(readLedgerItems).Should().BeTrue();

        decimal balance = softBank.LedgerItems.GetBalance(_owner, _context).Return();
        balance.Should().Be(450.30M);
    }
}