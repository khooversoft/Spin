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
using Toolbox.Orleans.Types;
using Toolbox.Types;

namespace SoftBank.sdk.test.Transactions;

public class TwoAccounts
{
    private readonly TestCluster _cluster;
    private readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);
    private readonly User _user1;
    private readonly User _user2;
    private readonly Contract _contract1;
    private readonly Contract _contract2;

    public TwoAccounts(ClusterFixture fixture)
    {
        _cluster = fixture.Cluster;

        _user1 = new User(_cluster, "owner1@company1.com", $"{SpinConstants.Schema.PrincipalKey}/test.com/owner1@company1.com");
        _user2 = new User(_cluster, "owner2@company2.com", $"{SpinConstants.Schema.PrincipalKey}/test.com/owner2@company2.com");
        _contract1 = new Contract(_cluster, $"{SpinConstants.Schema.SoftBank}/company1.com/Account1", _user1.OwnerId);
        _contract2 = new Contract(_cluster, $"{SpinConstants.Schema.SoftBank}/company1.com/Account2", _user2.OwnerId);
    }

    [Fact]
    public async Task TransferFunds()
    {
        await _user1.Delete(_context);
        await _user2.Delete(_context);
        await _contract1.Delete(_context);
        await _contract2.Delete(_context);

        await _user1.Createkey(_context);
        await _user2.Createkey(_context);

        await _contract1.CreateContract(_context);
        await _contract2.CreateContract(_context);

        var l1 = new LedgerItem { OwnerId = _user1.OwnerId, Description = "Ledger 1", Type = LedgerType.Credit, Amount = 100.0m };
        await _contract1.AddLedgerItem(l1, _context);



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

    private record User
    {
        private ISignatureActor _signatureActor;

        public User(TestCluster cluster, string ownerId, string keyId)
        {
            OwnerId = ownerId;
            KeyId = keyId;

            _signatureActor = cluster.GrainFactory.GetGrain<ISignatureActor>(KeyId);
        }

        public string OwnerId { get; }
        public string KeyId { get; }

        public async Task Createkey(ScopeContext context)
        {
            var request = new PrincipalKeyRequest
            {
                KeyId = KeyId,
                OwnerId = OwnerId,
                Audience = "test.com",
                Name = "test sign key",
            };

            await Delete(context);

            SpinResponse result = await _signatureActor.Create(request, context.TraceId);
            result.StatusCode.IsOk().Should().BeTrue();
        }

        public async Task Delete(ScopeContext context) => await _signatureActor.Delete(context.TraceId);
    }

    private record Contract
    {
        private readonly ISoftBankActor _softbank;

        public Contract(TestCluster cluster, string objectId, string ownerId)
        {
            ObjectId = objectId;
            OwnerId = ownerId;
            _softbank = cluster.GrainFactory.GetGrain<ISoftBankActor>(ObjectId);
        }

        public string ObjectId { get; } = null!;
        public string OwnerId { get; } = null!;

        public async Task CreateContract(ScopeContext context)
        {
            var request = new AccountDetail
            {
                ObjectId = ObjectId,
                OwnerId = OwnerId,
                Name = "name1",
            };

            await Delete(context);

            SpinResponse createResult = await _softbank.Create(request, context.TraceId);
            createResult.StatusCode.IsOk().Should().BeTrue(createResult.Error);
        }

        public async Task Delete(ScopeContext context) => await _softbank.Delete(context.TraceId);

        public async Task AddLedgerItem(LedgerItem ledgerItem, ScopeContext context)
        {
            var addResponse = await _softbank.AddLedgerItem(ledgerItem, context.TraceId);
            addResponse.StatusCode.IsOk().Should().BeTrue(addResponse.Error);
        }

        public async Task<decimal> GetBalance(ScopeContext context)
        {
            var balanceOption = await _softbank.GetBalance(context.TraceId);
            balanceOption.StatusCode.IsOk().Should().BeTrue();
            return balanceOption.Return();
        }
    }
}