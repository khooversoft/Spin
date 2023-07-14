using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Block;
using Toolbox.Security.Principal;
using Toolbox.Types;

namespace SoftBank.sdk.test;

public class SimpleTransactions
{
    private const string _owner = "user@domain.com";
    private readonly ObjectId _ownerObjectId = $"user/tenant/{_owner}".ToObjectId();
    private readonly PrincipalSignature _ownerSignature = new PrincipalSignature(_owner, _owner, "userBusiness@domain.com");
    private readonly PrincipalSignatureCollection _signCollection;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public SimpleTransactions()
    {
        _signCollection = new PrincipalSignatureCollection().Add(_ownerSignature);
    }

    [Fact]
    public async Task ConstructTest()
    {
        var softBank = await SoftBankAccount.Create(_owner, _ownerObjectId, _signCollection, _context).Return();
        Option signResult = await softBank.ValidateBlockChain(_signCollection, _context);
        signResult.StatusCode.IsOk().Should().BeTrue();
    }

    [Fact]
    public async Task ConstructWithAccountDetails()
    {
        var softBank = await SoftBankAccount.Create(_owner, _ownerObjectId, _signCollection, _context).Return();

        var accountDetail = new AccountDetail
        {
            ObjectId = _ownerObjectId.ToString(),
            OwnerId = _owner,
        };

        accountDetail.IsValid(_context.Location()).Should().BeTrue();

        BlockScalarStream<AccountDetail> stream = softBank.GetAccountDetailStream();
        stream.Add(await stream.CreateDataBlock(accountDetail, _owner).Sign(_signCollection, _context).Return());

        Option signResult = await softBank.ValidateBlockChain(_signCollection, _context);
        signResult.StatusCode.IsOk().Should().BeTrue();

        AccountDetail readAccountDetail = stream.Get().Return();
        (accountDetail == readAccountDetail).Should().BeTrue();
    }

    [Fact]
    public async Task ConstructWithLedgerItems()
    {
        var softBank = await SoftBankAccount.Create(_owner, _ownerObjectId, _signCollection, _context).Return();

        LedgerItem ledgerItem = new LedgerItem
        {
            Description = "Start of account",
            Type = LedgerType.Credit,
            Amount = 100,
        };

        ledgerItem.IsValid(_context.Location()).Should().BeTrue();

        BlockStream<LedgerItem> stream = softBank.GetLedgerStream();
        stream.Add(await stream.CreateDataBlock(ledgerItem, _owner).Sign(_signCollection, _context).Return());

        Option signResult = await softBank.ValidateBlockChain(_signCollection, _context);
        signResult.StatusCode.IsOk().Should().BeTrue();

        IReadOnlyList<LedgerItem> readledgerItem = stream.Get();
        readledgerItem.Count.Should().Be(1);
        (ledgerItem == readledgerItem.First()).Should().BeTrue();
    }
}