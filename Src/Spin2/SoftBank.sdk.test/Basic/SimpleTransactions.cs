using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using SoftBank.sdk.Models;
using Toolbox.Block;
using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Types;

namespace SoftBank.sdk.test.Basic;

public class SimpleTransactions
{
    private const string _owner = "user@domain.com";
    private const string _owner2 = "user2@domain.com";
    private readonly ObjectId _accountObjectId = $"contract/tenant/{_owner}/account1".ToObjectId();
    private readonly PrincipalSignature _ownerSignature = new PrincipalSignature(_owner, _owner, "userBusiness@domain.com");
    private readonly PrincipalSignature _ownerSignature2 = new PrincipalSignature(_owner2, _owner2, "userBusiness@domain.com");
    private readonly PrincipalSignatureCollection _signCollection;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);
    private readonly SoftBankFactory _softBankFactory;

    public SimpleTransactions()
    {
        _signCollection = new PrincipalSignatureCollection()
            .Add(_ownerSignature)
            .Add(_ownerSignature2);

        _softBankFactory = new SoftBankFactory(_signCollection, _signCollection, NullLoggerFactory.Instance);
    }

    [Fact]
    public async Task ConstructTest()
    {
        var softBank = await _softBankFactory.Create(_accountObjectId, _owner, _context).Return();
        Option signResult = await softBank.ValidateBlockChain(_signCollection, _context);
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

        accountDetail.IsValid(_context.Location()).Should().BeTrue();

        //BlockScalarStream<AccountDetail> stream = softBank.GetAccountDetailStream();
        //stream.Add(await stream.CreateDataBlock(accountDetail, _owner).Sign(_signCollection, _context).Return());

        //Option signResult = await softBank.ValidateBlockChain(_signCollection, _context);
        //signResult.StatusCode.IsOk().Should().BeTrue();

        //AccountDetail readAccountDetail = stream.Get().Return();
        //(accountDetail == readAccountDetail).Should().BeTrue();
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

        ledgerItem.IsValid(_context.Location()).Should().BeTrue();

        //BlockStream<LedgerItem> stream = softBank.GetLedgerStream();
        //stream.Add(await stream.CreateDataBlock(ledgerItem, _owner).Sign(_signCollection, _context).Return());

        //Option signResult = await softBank.ValidateBlockChain(_signCollection, _context);
        //signResult.StatusCode.IsOk().Should().BeTrue();

        //IReadOnlyList<LedgerItem> readledgerItem = stream.Get();
        //readledgerItem.Count.Should().Be(1);
        //(ledgerItem == readledgerItem.First()).Should().BeTrue();
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

        accountDetail.IsValid(_context.Location()).Should().BeTrue();

        //BlockScalarStream<AccountDetail> accountDetailStream = softBank.GetAccountDetailStream();
        //accountDetailStream.Add(await accountDetailStream.CreateDataBlock(accountDetail, _owner).Sign(_signCollection, _context).Return());

        //Option signResult = await softBank.ValidateBlockChain(_signCollection, _context);
        //signResult.StatusCode.IsOk().Should().BeTrue();


        //var ledgerItems = new[]
        //{
        //    new LedgerItem { OwnerId = _owner, Description = "Ledger 1", Type = LedgerType.Credit, Amount = 100.0m },
        //    new LedgerItem { OwnerId = _owner, Description = "Ledger 2", Type = LedgerType.Credit, Amount = 55.15m },
        //    new LedgerItem { OwnerId = _owner, Description = "Ledger 3", Type = LedgerType.Debit, Amount = 20.00m }
        //};

        //BlockStream<LedgerItem> ledgerStream = softBank.GetLedgerStream();
        //await ledgerItems
        //    .Select(x => ledgerStream.CreateDataBlock(x, _owner).Sign(_signCollection, _context).Return())
        //    .ForEachAsync(async x => ledgerStream.Add(await x));

        //signResult = await softBank.ValidateBlockChain(_signCollection, _context);
        //signResult.StatusCode.IsOk().Should().BeTrue();

        //AccountDetail readAccountDetail = accountDetailStream.Get().Return();
        //(accountDetail == readAccountDetail).Should().BeTrue();

        //IReadOnlyList<LedgerItem> readLedgerItems = ledgerStream.Get();
        //readLedgerItems.Count.Should().Be(3);
        //ledgerItems.SequenceEqual(readLedgerItems).Should().BeTrue();

        //decimal balance = softBank.GetBalance();
        //balance.Should().Be(135.15m);
    }

    [Fact]
    public async Task ConstructAccountWithLedgerItems2Signers()
    {
        var acl = new BlockAcl
        {
            Items = new BlockAccess[]
            {
                //new BlockAccess {BlockType = "collection:LedgerItem", PrincipalId = _owner2, Grant = true },
            },
        };

        var softBank = await _softBankFactory.Create(_accountObjectId, _owner, acl, _context).Return();

        var accountDetail = new AccountDetail
        {
            ObjectId = _accountObjectId.ToString(),
            OwnerId = _owner,
            Name = "Softbank 1",
        };

        accountDetail.IsValid(_context.Location()).Should().BeTrue();

        //BlockScalarStream<AccountDetail> accountDetailStream = softBank.GetAccountDetailStream();
        //accountDetailStream.Add(await accountDetailStream.CreateDataBlock(accountDetail, _owner).Sign(_signCollection, _context).Return());

        //Option signResult = await softBank.ValidateBlockChain(_signCollection, _context);
        //signResult.StatusCode.IsOk().Should().BeTrue();


        //var ledgerItems = new[]
        //{
        //    new LedgerItem { OwnerId = _owner, Description = "Ledger 1", Type = LedgerType.Credit, Amount = 100.0m },
        //    new LedgerItem { OwnerId = _owner, Description = "Ledger 2", Type = LedgerType.Credit, Amount = 55.15m },
        //    new LedgerItem { OwnerId = _owner, Description = "Ledger 3", Type = LedgerType.Debit, Amount = 20.00m }
        //};

        //BlockStream<LedgerItem> ledgerStream = softBank.GetLedgerStream();
        //await ledgerItems
        //    .Select(x => ledgerStream.CreateDataBlock(x, _owner).Sign(_signCollection, _context).Return())
        //    .ForEachAsync(async x => ledgerStream.Add(await x).ThrowOnError());

        //signResult = await softBank.ValidateBlockChain(_signCollection, _context);
        //signResult.StatusCode.IsOk().Should().BeTrue();

        //var ledgerItems2 = new[]
        //{
        //    new LedgerItem { OwnerId = _owner2, Description = "Ledger 1-2", Type = LedgerType.Credit, Amount = 200.0m },
        //    new LedgerItem { OwnerId = _owner2, Description = "Ledger 2-2", Type = LedgerType.Credit, Amount = 155.15m },
        //    new LedgerItem { OwnerId = _owner2, Description = "Ledger 3-2", Type = LedgerType.Debit, Amount = 40.00m }
        //};

        //await ledgerItems2
        //    .Select(x => ledgerStream.CreateDataBlock(x, _owner2).Sign(_signCollection, _context).Return())
        //    .ForEachAsync(async x => ledgerStream.Add(await x).ThrowOnError());

        //signResult = await softBank.ValidateBlockChain(_signCollection, _context);
        //signResult.StatusCode.IsOk().Should().BeTrue();

        //AccountDetail readAccountDetail = accountDetailStream.Get().Return();
        //(accountDetail == readAccountDetail).Should().BeTrue();

        //IReadOnlyList<LedgerItem> readLedgerItems = ledgerStream.Get();
        //readLedgerItems.Count.Should().Be(6);
        //ledgerItems.Concat(ledgerItems2).SequenceEqual(readLedgerItems).Should().BeTrue();

        //decimal balance = softBank.GetBalance();
        //balance.Should().Be(450.30M);
    }
}