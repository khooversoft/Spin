using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Orleans.TestingHost;
using SoftBank.sdk.Models;
using SoftBank.sdk.test.Application;
using SpinCluster.sdk.Actors.Signature;
using SpinCluster.sdk.Actors.SoftBank;
using SpinCluster.sdk.Application;
using Toolbox.Block;
using Toolbox.Orleans.Types;
using Toolbox.Types;

namespace SoftBank.sdk.test;

public class StandardTransactions : IClassFixture<ClusterFixture>
{
    private readonly TestCluster _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    public StandardTransactions(ClusterFixture fixture)
    {
        _cluster = fixture.Cluster;
    }

    [Fact]
    public async Task CreateBankAccountAndLedgerItem()
    {
        const string ownerId = "signKey@test.com";
        const string objectId = $"{SpinConstants.Schema.SoftBank}/test.com/StandardTransactions/{ownerId}";
        const string keyId = $"{SpinConstants.Schema.PrincipalKey}/test.com/{ownerId}";
        const string name = "name1";

        ISoftBankActor softBankActor = _cluster.GrainFactory.GetGrain<ISoftBankActor>(objectId);
        ISignatureActor signatureActor = _cluster.GrainFactory.GetGrain<ISignatureActor>(keyId);

        await softBankActor.Delete(_context.TraceId);
        await signatureActor.Delete(_context.TraceId);

        await CreateKeys(signatureActor, keyId, ownerId);

        var request = new AccountDetail 
        {
            ObjectId = objectId,
            OwnerId = ownerId,
            Name = name,
        };

        SpinResponse createResult = await softBankActor.Create(request, _context.TraceId);
        createResult.StatusCode.IsOk().Should().BeTrue(createResult.Error);

        var ledgerItem = new LedgerItem { OwnerId = ownerId, Description = "Ledger 1", Type = LedgerType.Credit, Amount = 100.0m };

        SpinResponse ledgerItemResponse = await softBankActor.AddLedgerItem(ledgerItem, _context.TraceId);
        ledgerItemResponse.StatusCode.IsOk().Should().BeTrue();

        SpinResponse<AccountDetail> accountDetails = await softBankActor.GetBankDetails(_context.TraceId);
        accountDetails.StatusCode.IsOk().Should().BeTrue(accountDetails.Error);
        (request == accountDetails.Return()).Should().BeTrue("not equal");

        SpinResponse<IReadOnlyList<LedgerItem>> ledgerItems = await softBankActor.GetLedgerItems(_context.TraceId);
        ledgerItems.StatusCode.IsOk().Should().BeTrue(ledgerItems.Error);
        ledgerItems.Return().Count.Should().Be(1);
        (ledgerItems.Return()[0] == ledgerItem).Should().BeTrue();

        SpinResponse<decimal> balanceResponse = await softBankActor.GetBalance(_context.TraceId);
        balanceResponse.StatusCode.IsOk().Should().BeTrue();
        balanceResponse.Return().Should().Be(100.0m);

        // Clean up
        var deleteResponse = await softBankActor.Delete(_context.TraceId);
        var signatureResponse = await signatureActor.Delete(_context.TraceId);
        deleteResponse.StatusCode.IsOk().Should().BeTrue();
        signatureResponse.StatusCode.IsOk().Should().BeTrue();
    }

    [Fact]
    public async Task CreateBankAccountAndMultipleLedgerItems()
    {
        const string ownerId = "owner1@test.com";
        const string objectId = $"{SpinConstants.Schema.SoftBank}/test.com/StandardTransactions/{ownerId}";
        const string keyId = $"{SpinConstants.Schema.PrincipalKey}/test.com/{ownerId}";
        const string name = "name1";

        ISoftBankActor softBankActor = _cluster.GrainFactory.GetGrain<ISoftBankActor>(objectId);
        ISignatureActor signatureActor = _cluster.GrainFactory.GetGrain<ISignatureActor>(keyId);

        await softBankActor.Delete(_context.TraceId);
        await signatureActor.Delete(_context.TraceId);

        await CreateKeys(signatureActor, keyId, ownerId);

        var request = new AccountDetail 
        {
            ObjectId = objectId,
            OwnerId = ownerId,
            Name = name,
        };

        SpinResponse createResult = await softBankActor.Create(request, _context.TraceId);
        createResult.StatusCode.IsOk().Should().BeTrue(createResult.Error);

        var newItems = new[]
{
            new LedgerItem { OwnerId = ownerId, Description = "Ledger 1", Type = LedgerType.Credit, Amount = 100.0m },
            new LedgerItem { OwnerId = ownerId, Description = "Ledger 2", Type = LedgerType.Credit, Amount = 55.15m },
            new LedgerItem { OwnerId = ownerId, Description = "Ledger 3", Type = LedgerType.Debit, Amount = 20.00m }
        };

        foreach (var item in newItems)
        {
            var addResponse = await softBankActor.AddLedgerItem(item, _context.TraceId);
            addResponse.StatusCode.IsOk().Should().BeTrue(addResponse.Error);
        }

        SpinResponse<AccountDetail> accountDetails = await softBankActor.GetBankDetails(_context.TraceId);
        accountDetails.StatusCode.IsOk().Should().BeTrue(accountDetails.Error);
        (request == accountDetails.Return()).Should().BeTrue("not equal");

        SpinResponse<IReadOnlyList<LedgerItem>> ledgerItems = await softBankActor.GetLedgerItems(_context.TraceId);
        ledgerItems.StatusCode.IsOk().Should().BeTrue(ledgerItems.Error);
        ledgerItems.Return().Count.Should().Be(newItems.Length);
        Enumerable.SequenceEqual(newItems, ledgerItems.Return()).Should().BeTrue();

        SpinResponse<decimal> balanceResponse = await softBankActor.GetBalance(_context.TraceId);
        balanceResponse.StatusCode.IsOk().Should().BeTrue();
        balanceResponse.Return().Should().Be(135.15m);

        // Clean up
        var deleteResponse = await softBankActor.Delete(_context.TraceId);
        var signatureResponse = await signatureActor.Delete(_context.TraceId);
        deleteResponse.StatusCode.IsOk().Should().BeTrue();
        signatureResponse.StatusCode.IsOk().Should().BeTrue();
    }

    private async Task CreateKeys(ISignatureActor signatureActor, string keyId, string ownerId)
    {
        var request = new PrincipalKeyRequest
        {
            KeyId = keyId,
            OwnerId = ownerId,
            Audience = "test.com",
            Name = "test sign key",
        };

        await signatureActor.Delete(_context.TraceId);

        SpinResponse result = await signatureActor.Create(request, _context.TraceId);
        result.StatusCode.IsOk().Should().BeTrue();
    }
}
