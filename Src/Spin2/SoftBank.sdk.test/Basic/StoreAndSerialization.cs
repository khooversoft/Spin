using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SoftBank.sdk.Models;
using Toolbox.Block;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Types;

namespace SoftBank.sdk.test.Basic;

public class StoreAndSerialization
{
    private const string _owner = "user@domain.com";
    private readonly ObjectId _ownerObjectId = $"user/tenant/{_owner}".ToObjectId();
    private readonly PrincipalSignature _ownerSignature = new PrincipalSignature(_owner, _owner, "userBusiness@domain.com");
    private readonly PrincipalSignatureCollection _signCollection;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);
    private readonly SoftBankFactory _softBankFactory;

    public StoreAndSerialization()
    {
        _signCollection = new PrincipalSignatureCollection().Add(_ownerSignature);
    
        _softBankFactory = new SoftBankFactory(_signCollection, _signCollection, NullLoggerFactory.Instance);
    }

    [Fact]
    public async Task InitialStateStore()
    {
        SoftBankAccount softBankAccount = await CreateAccount();

        BlobPackage blob = softBankAccount.ToBlobPackage();
        blob.Validate(_context.Location()).IsValid.Should().BeTrue();

        SoftBankAccount readSoftBankAccount = await _softBankFactory.Create(blob, _context).Return();
        Option signResult = await readSoftBankAccount.ValidateBlockChain(_signCollection, _context);
        signResult.StatusCode.IsOk().Should().BeTrue();
    }

    private async Task<SoftBankAccount> CreateAccount()
    {
        var softBank = await _softBankFactory.Create(_ownerObjectId, _owner, _context).Return();

        var accountDetail = new AccountDetail
        {
            ObjectId = _ownerObjectId.ToString(),
            OwnerId = _owner,
            Name = "Softbank 1",
        };

        accountDetail.IsValid(_context.Location()).Should().BeTrue();

        BlockScalarStream<AccountDetail> accountDetailStream = softBank.GetAccountDetailStream();
        accountDetailStream.Add(await accountDetailStream.CreateDataBlock(accountDetail, _owner).Sign(_signCollection, _context).Return());

        Option signResult = await softBank.ValidateBlockChain(_signCollection, _context);
        signResult.StatusCode.IsOk().Should().BeTrue();


        var ledgerItems = new[]
        {
            new LedgerItem { OwnerId = _owner, Description = "Ledger 1", Type = LedgerType.Credit, Amount = 100.0m },
            new LedgerItem { OwnerId = _owner, Description = "Ledger 2", Type = LedgerType.Credit, Amount = 55.15m },
            new LedgerItem { OwnerId = _owner, Description = "Ledger 3", Type = LedgerType.Debit, Amount = 20.00m }
        };

        BlockStream<LedgerItem> ledgerStream = softBank.GetLedgerStream();
        await ledgerItems
            .Select(x => ledgerStream.CreateDataBlock(x, _owner).Sign(_signCollection, _context).Return())
            .ForEachAsync(async x => ledgerStream.Add(await x));

        signResult = await softBank.ValidateBlockChain(_signCollection, _context);
        signResult.StatusCode.IsOk().Should().BeTrue();

        AccountDetail readAccountDetail = accountDetailStream.Get().Return();
        (accountDetail == readAccountDetail).Should().BeTrue();

        IReadOnlyList<LedgerItem> readLedgerItems = ledgerStream.Get();
        readLedgerItems.Count.Should().Be(3);
        ledgerItems.SequenceEqual(readLedgerItems).Should().BeTrue();

        decimal balance = softBank.GetBalance();
        balance.Should().Be(135.15m);

        return softBank;
    }
}
