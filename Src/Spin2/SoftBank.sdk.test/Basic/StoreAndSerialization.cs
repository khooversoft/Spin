using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SoftBank.sdk.Application;
using SoftBank.sdk.Models;
using Toolbox.Block;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Security.Principal;
using Toolbox.Types;

namespace SoftBank.sdk.test.Basic;

public class StoreAndSerialization
{
    private static readonly PrincipalId _owner = "user@domain.com";
    private static readonly ObjectId _ownerObjectId = $"user/tenant/{_owner}";
    private static readonly PrincipalSignature _ownerSignature = new PrincipalSignature(_owner, _owner, "userBusiness@domain.com");
    private readonly PrincipalSignatureCollection _signCollection;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);
    private readonly SoftBankFactory _softBankFactory;

    public StoreAndSerialization()
    {
        _signCollection = new PrincipalSignatureCollection().Add(_ownerSignature);

        IServiceProvider service = new ServiceCollection()
            .AddLogging()
            .AddSoftBank()
            .AddSingleton<ISign>(_signCollection)
            .AddSingleton<ISignValidate>(_signCollection)
            .BuildServiceProvider();

        _softBankFactory = service.GetRequiredService<SoftBankFactory>();
    }

    [Fact]
    public async Task InitialStateStore()
    {
        SoftBankAccount softBankAccount = await CreateAccount();

        BlobPackage blob = softBankAccount.ToBlobPackage();
        blob.Validate().IsOk().Should().BeTrue();

        SoftBankAccount readSoftBankAccount = await _softBankFactory.Create(blob, _context).Return();
        Option signResult = await readSoftBankAccount.ValidateBlockChain(_context);
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

        accountDetail.Validate().IsOk().Should().BeTrue();

        var addDetailResult = await softBank.AccountDetail.Set(accountDetail, _context);
        addDetailResult.StatusCode.IsOk().Should().BeTrue();

        Option signResult = await softBank.ValidateBlockChain(_context);
        signResult.StatusCode.IsOk().Should().BeTrue();


        var ledgerItems = new[]
        {
            new LedgerItem { OwnerId = _owner, Description = "Ledger 1", Type = LedgerType.Credit, Amount = 100.0m },
            new LedgerItem { OwnerId = _owner, Description = "Ledger 2", Type = LedgerType.Credit, Amount = 55.15m },
            new LedgerItem { OwnerId = _owner, Description = "Ledger 3", Type = LedgerType.Debit, Amount = 20.00m }
        };

        foreach (var item in ledgerItems)
        {
            var addResult = await softBank.LedgerItems.Add(item, _context);
            addResult.StatusCode.IsOk().Should().BeTrue();
        }

        signResult = await softBank.ValidateBlockChain(_context);
        signResult.StatusCode.IsOk().Should().BeTrue();

        AccountDetail readAccountDetail = softBank.AccountDetail.Get(_owner, _context).Return();
        (accountDetail == readAccountDetail).Should().BeTrue();

        BlockReader<LedgerItem> readLedgerItems = softBank.LedgerItems.GetReader(_owner, _context).Return();
        readLedgerItems.Count.Should().Be(3);
        ledgerItems.SequenceEqual(readLedgerItems.List()).Should().BeTrue();

        decimal balance = softBank.LedgerItems.GetBalance(_owner, _context).Return();
        balance.Should().Be(135.15m);

        return softBank;
    }
}
