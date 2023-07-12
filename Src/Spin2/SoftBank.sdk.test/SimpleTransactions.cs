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
    public async Task ConstructTestWithAccountDetails()
    {
        var accountDetail = new AccountDetail
        {
            ObjectId = _ownerObjectId.ToString(),
            OwnerId = _owner,
        };

        var softBank = await SoftBankAccount.Create(_owner, _ownerObjectId, _signCollection, _context).Return();

        BlockScalarStream<AccountDetail> stream = softBank.GetAccountDetailStream();
        stream.Add(await stream.CreateDataBlock(accountDetail, _owner).Sign(_signCollection, _context).Return());

        Option signResult = await softBank.ValidateBlockChain(_signCollection, _context);
        signResult.StatusCode.IsOk().Should().BeTrue();

        AccountDetail readAccountDetail = stream.Get().Return();
        (accountDetail == readAccountDetail).Should().BeTrue();
    }
}